using System.Data;
using System.Data.SqlClient;

namespace Grabex;

public class Repository(SqlDb db)
{
	/// <summary>
	/// Uložení scraped company
	/// </summary>
	/// <param name="company">Scraped company ID</param>
	/// <returns></returns>
	public async Task<int> ScrapedCompanyInsertAsync(ScrapedCompany company)
	{
		var parms = new[]
		{
			new SqlParameter("@name", company.Name),
			new SqlParameter("@scrapeId", company.ScrapeId),
			new SqlParameter("@categoryId", company.CategoryId),
			new SqlParameter("@path", company.Path),
			new SqlParameter("@description", company.Description),
			new SqlParameter("@firmyCzInternalId", company.FirmyCzInternalId)
		};

		return await db.InsertAndGetIdAsync("""
			INSERT INTO ScrapedCompany ([Name], ScrapeId, CategoryId, Path, Description, FirmyCzInternalId) 
			VALUES (@name, @scrapeId, @categoryId, @path, @description, @firmyCzInternalId)
			""", parms);
	}

	public async Task<int> ScrapeInsertAsync()
	{
		return await db.InsertAndGetIdAsync("INSERT INTO Scrape DEFAULT VALUES");
	}

	public async Task<IEnumerable<(int Id, string Name, string Path)>> GetCategoriesAsync()
	{
		List<(int Id, string Name, string Path)> categories = [];
		var categs = await db.ExecuteQueryAsync("SELECT Id, Name, Path FROM Category");
		foreach (DataRow categ in categs.Rows)
		{
			int id = (int)categ["Id"];
			string name = (string)categ["Name"];
			string path = (string)categ["Path"];

			categories.Add((Id: id, Name: name, Path: path ));
		}

		return categories;
	}

	public async Task<int> ScrapedCompanyInfoInsertAsync(int scrapedCompanyId, string label, string value)
	{
		var parms = new[]
		{
			new SqlParameter("@scrapedCompanyId", scrapedCompanyId),
			new SqlParameter("@label", label),
			new SqlParameter("@value", value),
		};

		return await db.InsertAndGetIdAsync("""
			INSERT INTO ScrapedCompanyInfo (ScrapedCompanyId, [Label], [Value]) 
			VALUES (@scrapedCompanyId, @label, @value)
			""", parms);
	}

	public async Task<int> ScrapedCompanyTagInsertAsync(int scrapedCompanyId, string name, string path)
	{
		var parms = new[]
		{
			new SqlParameter("@scrapedCompanyId", scrapedCompanyId),
			new SqlParameter("@name", name),
			new SqlParameter("@path", path),
		};

		return await db.InsertAndGetIdAsync("""
			INSERT INTO ScrapedCompanyTag (ScrapedCompanyId, [Name], [Path]) 
			VALUES (@scrapedCompanyId, @name, @path)
			""", parms);
	}

	public async Task<int> ScrapedCompanyCategoryInsertAsync(int scrapedCompanyId, string name, string path)
	{
		var parms = new[]
		{
			new SqlParameter("@scrapedCompanyId", scrapedCompanyId),
			new SqlParameter("@name", name),
			new SqlParameter("@path", path),
		};

		return await db.InsertAndGetIdAsync("""
			INSERT INTO ScrapedCompanyCategory (ScrapedCompanyId, [Name], [Path]) 
			VALUES (@scrapedCompanyId, @name, @path)
			""", parms);
	}
}
