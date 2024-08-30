using Microsoft.Extensions.Configuration;

namespace Grabex;

public class Config
{
	public (
		string? connectionString, 
		int requestIntervalMin, 
		int requestIntervalMax,
		int scrapeCompanyErrorsLimit
		) GetConfig(string connectionName)
	{
		const string configFilename = "appsettings.json";
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile(configFilename, optional: true, reloadOnChange: true)
			.Build();

		string? connectionString = configuration.GetConnectionString(connectionName);

		int requestIntervalMin = configuration.GetValue<int>("RequestIntervalMin");
		int requestIntervalMax = configuration.GetValue<int>("RequestIntervalMax");

		if (requestIntervalMin == default)
		{
			requestIntervalMin = 320;
		}

		if (requestIntervalMin > requestIntervalMax)
		{
			requestIntervalMax = requestIntervalMin;
		}

		int scrapeCompanyErrorsLimit = configuration.GetValue<int>("ScrapeCompanyErrorsLimit");
		if (scrapeCompanyErrorsLimit == default)
		{
			scrapeCompanyErrorsLimit = 10;
		}

		return (connectionString, requestIntervalMin, requestIntervalMax, scrapeCompanyErrorsLimit);
	}
}
