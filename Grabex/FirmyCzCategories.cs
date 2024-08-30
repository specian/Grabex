using System.Collections.Immutable;

namespace Grabex;

/// <summary>
/// Třída, která zná kategorie z firmy.cz
/// </summary>
public class FirmyCzCategories(Repository repo)
{
	protected readonly ImmutableArray<(int Id, string Name, string Path)> categoriesCodeTable = repo.GetCategoriesAsync().Result.ToImmutableArray();

	/// <summary>
	/// Vrátí ID kategorie podle URL
	/// </summary>
	/// <param name="path">Cesta za https://www.firmy.cz, tedy např. "/Auto-moto"</param>
	/// <returns></returns>
	public int GetCategoryIdFromUrl(string path)
	{
		var categoryFound = FuzzySharp.Process.ExtractOne(path, categoriesCodeTable.Select(c => c.Path));
		return categoriesCodeTable.FirstOrDefault(c => c.Path == categoryFound.Value).Id;
	}
}