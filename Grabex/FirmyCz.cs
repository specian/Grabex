using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Grabex;

/// <summary>
/// Třída, která umí scrapovat firmy z firmy.cz
/// </summary>
public class FirmyCz(FakeBrowser fakeBrowser, int requestIntervalMin, int requestIntervalMax, ILogger<FirmyCz>? logger)
{
	const string FirmyCzUrl = "https://www.firmy.cz/";

	protected FakeBrowser fakeBrowser = fakeBrowser;
	protected HttpClient httpClient = fakeBrowser.GetBrowser();

	public HashSet<string> CompanyUrls { get; protected set; } = [];

	/// <summary>
	/// Hlavní metoda, která scrapuje data z firmy.cz
	/// </summary>
	/// <returns></returns>
	public async Task ScrapeAsync()
	{
		CompanyUrls = [];
		foreach (var (mainCategoryName, mainCategoryUrl) in await ScrapeMainCategoriesAsync())
		{
			logger?.LogInformation("Scraping main category {mainCategoryName} ({mainCategoryUrl})", mainCategoryName, mainCategoryUrl);
			await ScrapeCompanyUrlsFromCategoryAsync(mainCategoryUrl);
#if DEBUG
			break;
#endif
		}
	}

	/// <summary>
	/// Umí přečíst detail firmy
	/// </summary>
	/// <param name="url">URL detailu firmy</param>
	/// <returns></returns>
	public async Task<ScrapedCompany> ScrapeCompanyAsync(string url)
	{
		HttpResponseMessage response = await httpClient.GetAsync(url);
		string pageContents = await response.Content.ReadAsStringAsync();
		HtmlAgilityPack.HtmlDocument detailPage = new HtmlAgilityPack.HtmlDocument();
		detailPage.LoadHtml(pageContents);

		return new ScrapedCompany()
		{
			Name = GetName(detailPage),
			Address = GetAddress(detailPage),
			Description = GetDescription(detailPage),
			BasicInfos = GetBasicInfos(detailPage),
			FirmyCzInternalId = GetInternalIdFromUrl(url),
			Tags = GetTags(detailPage),
			Categories = GetCategories(detailPage)
		};
	}

	/// <summary>
	/// Vrátí názvy a URL hlavních kategorií na firmy.cz
	/// </summary>
	/// <param name="client"></param>
	/// <returns>Názvy a URL hlavních kategorií</returns>
	protected async Task<IEnumerable<(string Name, string Url)>> ScrapeMainCategoriesAsync()
	{
		HttpResponseMessage response = await httpClient.GetAsync(FirmyCzUrl);
		string pageContents = await response.Content.ReadAsStringAsync();

		// Načtení HTML dokumentu do HtmlAgilityPack
		var htmlDoc = new HtmlAgilityPack.HtmlDocument();
		htmlDoc.LoadHtml(pageContents);
		return htmlDoc.DocumentNode.SelectNodes("//div[@class='mainCategories']//h3/a")
			.Select(c => (Name: c.InnerText, Url: c.Attributes["href"].Value));
	}

	/// <summary>
	/// Scrapuje URL všechny firmy z jedné hlavní kategorie dodané jako URL (funguje i pro podkategorie).
	/// Projde všechny stránky stránkování.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="categoryUrl"></param>
	/// <returns></returns>
	protected async Task ScrapeCompanyUrlsFromCategoryAsync(string categoryUrl)
	{
		var (companiesOnPage, companiesTotal, urls) = await ScrapeOnePageAsync(categoryUrl, 1);
		if (companiesOnPage == 0 || !urls.Any())
		{
			throw new Exception($"No premises on page. URL: {categoryUrl}, page: 1");
		}

		CompanyUrls.UnionWith(urls);
		int pagesCount = (int)Math.Ceiling((double)companiesTotal / companiesOnPage);
		for (int page = 2; page <= pagesCount; page++)
		{
			var (_, _, pageUrls) = await ScrapeOnePageAsync(categoryUrl, page);
			CompanyUrls.UnionWith(pageUrls);
#if DEBUG
			break;
#endif
		}
	}

	/// <summary>
	/// Scrapuje jednu stránku s firmami. Vrací počet firem na stránce, celkový počet firem a URL firem.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="categoryUrl"></param>
	/// <param name="page"></param>
	/// <returns>Počet firem na stránce, celkový počet firem na všech stránkách, seznam URL firem na stránce.</returns>
	protected async Task<(int companiesOnPage, int companiesTotal, IEnumerable<string> companyUrls)> ScrapeOnePageAsync(string categoryUrl, int page)
	{
		logger?.LogInformation("Scraping page {categoryUrl} page {page}", categoryUrl, page);
		await Task.Delay(Random.Shared.Next(requestIntervalMin, requestIntervalMax));
		HttpResponseMessage response = await httpClient.GetAsync(categoryUrl + (page > 1 ? $"?page={page}" : ""));
		string pageContents = await response.Content.ReadAsStringAsync();
		var htmlDoc = new HtmlAgilityPack.HtmlDocument();
		htmlDoc.LoadHtml(pageContents);

		var premises = htmlDoc.DocumentNode.SelectNodes("//div[@class='premiseList']//h3/a");
		int companiesOnPage = premises?.Count ?? 0;
		if (companiesOnPage == 0)
		{
			logger?.LogWarning("No premises on page. URL: {categoryUrl}, page: {page}", categoryUrl, page);
			return (0, 0, []);
		}

		var companiesTotalStr = htmlDoc.DocumentNode.SelectNodes("//div[@class='pagingResults']//strong")[1].InnerText;
		_ = int.TryParse(companiesTotalStr, out int companiesTotal);
		var companyUrls = htmlDoc.DocumentNode.SelectNodes("//div[@class='premiseList']//h3/a")
			.Select(c => c.Attributes["href"].Value);
		return (companiesOnPage, companiesTotal, companyUrls);
	}

	protected string GetName(HtmlDocument detailPage)
	{
		return detailPage.DocumentNode.SelectNodes("//div[@class='container']/div[@class='content detail']//div[@class='detailHead']/div[@class='detailBox']/h1")[0].InnerText;
	}

	protected string GetAddress(HtmlDocument detailPage)
	{
		return detailPage.DocumentNode.SelectNodes("//div[@class='container']/div[@class='content detail']//div[@class='detailHead']/div[@class='detailBox']/div/text()[1]")[0].InnerText;
	}

	protected string GetDescription(HtmlAgilityPack.HtmlDocument detailPage)
	{
		return detailPage.DocumentNode.SelectNodes("//div[@class='container']/div[@class='content detail']//div[@class='detailDescription']/p")[0].InnerText;
	}

	protected IEnumerable<(string Label, string Value)> GetBasicInfos(HtmlDocument detailPage)
	{
		var basicInfoNodes = detailPage.DocumentNode.SelectNodes("//div[@class='container']/div[@class='content detail']//div[@class='detailBasicInfo']/*");

		var basicInfos = new List<(string, string)>();
		int i = 0;
		while (i < basicInfoNodes.Count - 1)
		{
			basicInfos.Add((basicInfoNodes[i].InnerText, basicInfoNodes[i + 1].InnerHtml));
			i += 2;
		}

		return basicInfos;
	}

	protected int? GetInternalIdFromUrl(string url)
	{
		Match match = Regex.Match(url, @"\/detail\/(\d+)-");
		return match.Success ? int.Parse(match.Groups[1].Value) : null;
	}

	protected IEnumerable<(string name, string url)> GetTags(HtmlDocument detailPage)
	{
		var lis = detailPage.DocumentNode.SelectNodes("//div[@class='detailLists']/div[contains(concat(' ', normalize-space(@class), ' '), ' ltag ')]//ul/li");
		var tags = lis?.Select(t => (t.InnerText, t.SelectSingleNode("a")?.Attributes["href"].Value[28..] ?? ""));
		return tags ?? [];
	}

	protected IEnumerable<(string Name, string Path)> GetCategories(HtmlDocument detailPage)
	{
		var lis = detailPage.DocumentNode.SelectNodes("//div[@class='detailLists']/div[contains(concat(' ', normalize-space(@class), ' '), ' lcat ')]//ul/li");
		var categs = lis?.Select(t => (t.InnerText, t.SelectSingleNode("a")?.Attributes["href"].Value[21..] ?? ""));
		return categs ?? [];
	}
}