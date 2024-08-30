namespace Grabex.Test;

public class FirmyCzTests : FirmyCz
{
	public FirmyCzTests() : base(new FakeBrowserEdge(), 320, 530, null) { }

	[Fact]
	public async Task ScrapeMainCategories_TestAsync()
	{
		var mainCategories = await ScrapeMainCategoriesAsync();
		Assert.Equal(12, mainCategories.Count());
	}

	[Fact]
	public async Task ScrapeCompanyUrlsFromCategory_TestAsync()
	{
		await ScrapeCompanyUrlsFromCategoryAsync("https://www.firmy.cz/Eroticke_firmy/Prodejci-erotickeho-zbozi");
		Assert.True(CompanyUrls.Count > 24 && CompanyUrls.Count < 32);
	}

	[Fact]
	public async Task ScrapeOnePage_TestAsync()
	{
		var (companiesOnPage, companiesTotal, companyUrls) = await ScrapeOnePageAsync("https://www.firmy.cz/Eroticke_firmy/Prodejci-erotickeho-zbozi", 1);

		Assert.True(companiesOnPage > 12 && companiesOnPage < 16);
		var urlsCount = companyUrls.Count();
		Assert.True(urlsCount > 12 && urlsCount < 16);
		Assert.True(companiesTotal > 40);
	}

	// Dočasné
	[Fact]
	public async Task ScrapeCompanyAsync_TestAsync()
	{
		//ScrapedCompany company1 = await ScrapeCompanyAsync("https://www.firmy.cz/detail/13592686-automatic-choice-ostrava-marianske-hory.html");
		//ScrapedCompany company2 = await ScrapeCompanyAsync("https://www.firmy.cz/detail/13385707-autopalace-pop-airport-tuchomerice.html");
		//ScrapedCompany company3 = await ScrapeCompanyAsync("https://www.firmy.cz/detail/12733688-penzion-u-farmare-chotoviny-moravec.html");
		//ScrapedCompany company4 = await ScrapeCompanyAsync("https://www.firmy.cz/detail/12852139-pasta-pizza-hradek-nad-nisou.html");
		ScrapedCompany company5 = await ScrapeCompanyAsync("https://www.firmy.cz/detail/2059354-sushi-restaurace-made-in-japan-praha-stare-mesto.html");
		ScrapedCompany company6 = await ScrapeCompanyAsync("https://www.firmy.cz/detail/1944092-pivovar-hasic-bruntal.html");
	}
}