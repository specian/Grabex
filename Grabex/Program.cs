using Grabex;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

// ----Konfigurace---------------------------------------------------------------------------
const string ConnectionName = "Milasntb";
//const string ConnectionName = "Argal";

// ----Příprava------------------------------------------------------------------------------
(
	string? connectionString, 
	int requestIntervalMin, 
	int requestIntervalMax,
	int scrapeCompanyErrorsLimit
) 
	= new Config().GetConfig(ConnectionName);

if (connectionString is null)
{
	Console.WriteLine($"Connection string {ConnectionName} not found in configuration file.");
	Environment.ExitCode = 1; 
	return;
}

// Konfigurace Serilog loggeru
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.WriteTo.File("Logs/grabex-.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();

// Nastavení poskytovatele služeb s logováním
var serviceProvider = new ServiceCollection()
	.AddLogging(loggingBuilder =>
	{
		loggingBuilder.ClearProviders();
		loggingBuilder.AddSerilog();
	})
	.BuildServiceProvider();

// Získání loggerů
ILogger<Program> logger = serviceProvider.GetService<ILogger<Program>>()!;
ILogger<FirmyCz> loggerFirmyCz = serviceProvider.GetService<ILogger<FirmyCz>>()!;

// Repository
SqlDb db = new(connectionString!);
Repository repo = new(db);


// ----Vlastní scrape------------------------------------------------------------------------
// Kategorie
logger.LogInformation("Begin");
logger.LogInformation("Scraping categories");
using FakeBrowser fakeBrowser = new FakeBrowserEdge();
FirmyCz firmyCz = new(fakeBrowser, requestIntervalMin, requestIntervalMax, loggerFirmyCz);
try
{
	await firmyCz.ScrapeAsync();
}
catch (Exception ex)
{
	logger.LogError(ex, "Error while scraping categories");
	Environment.ExitCode = 1;
	return;
}
logger.LogInformation("Scraping {urlCount} URLs", firmyCz.CompanyUrls.Count);
Console.WriteLine();

// Uložení scrape ID
int scrapeId;
try
{
	scrapeId = await repo.ScrapeInsertAsync();
}
catch (Exception ex)
{
	logger.LogError(ex, "Error while storing scrape ID (probably SQL connection problem).");
	Environment.ExitCode = 1;
	return;
}

logger.LogInformation("Scrape ID: {scrapeId}", scrapeId);
Console.WriteLine();

// Jednotlivé firmy
logger.LogInformation("Scraping companies");
FirmyCzCategories firmyCzCategories = new (repo);
int scrapeCompanyErrors = 0;
foreach (var url in firmyCz.CompanyUrls)
{
	await Task.Delay(new Random().Next(requestIntervalMin, requestIntervalMax));
	logger.LogInformation("Scraping company {url}", url);
	string path = new Uri(url).AbsolutePath;

	try
	{
		ScrapedCompany company = await firmyCz.ScrapeCompanyAsync(url);

		// Doplnění zbývajících údajů
		company.ScrapeId = scrapeId;
		company.CategoryId = firmyCzCategories.GetCategoryIdFromUrl(path);
		company.Path = path;
		company.Id = await repo.ScrapedCompanyInsertAsync(company);

		// Uložení basic info
		string trueLabel = "";
		foreach (var (label, value) in company.BasicInfos!)
		{
			// Pokud je label prázdný, použije se předchozí
			if (!string.IsNullOrWhiteSpace(label))
			{
				trueLabel = label;
			}

			await repo.ScrapedCompanyInfoInsertAsync(company.Id.Value, trueLabel, value);
		}

		// Uložení štítků
		foreach (var (tagName, tagPath) in company.Tags!)
		{
			await repo.ScrapedCompanyTagInsertAsync(company.Id.Value, tagName, tagPath);
		}

		// Uložení kategorií
		foreach (var (categName, categPath) in company.Categories!)
		{
			await repo.ScrapedCompanyCategoryInsertAsync(company.Id.Value, categName, categPath);
		}
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Error while scraping company {url}", url);
		if (++scrapeCompanyErrors > scrapeCompanyErrorsLimit)
		{
			logger.LogError("Too many errors while scraping companies. Exiting.");
			Environment.ExitCode = 1;
			return;
		}
	}
}

logger.LogInformation("End");
Log.CloseAndFlush();
Environment.ExitCode = 0;
