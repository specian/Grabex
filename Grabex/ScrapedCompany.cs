namespace Grabex;

public class ScrapedCompany
{
	public int? Id { get; set; }
	public int? ScrapeId { get; set; }
	public int? CategoryId { get; set; }
	public string? Path { get; set; }
	public string? Name { get; set; }
	public string? Address { get; set; }
	public int? FirmyCzInternalId { get; set; }
	public string? Description { get; set; }
	public IEnumerable<(string Label, string Value)>? BasicInfos { get; set; }
	public IEnumerable<(string Name, string Path)>? Tags { get; set; }
	public IEnumerable<(string Name, string Path)>? Categories { get; set; }
}