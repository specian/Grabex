namespace Grabex;

public class FakeBrowserEdge : FakeBrowser
{
	public override HttpClient GetBrowser()
	{
		SetHeadersEdge();
		return client;
	}

	protected void SetHeadersEdge()
	{
		client.DefaultRequestHeaders.Add("Connection", "keep-alive");
		client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.59");
		client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
		client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
		client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
		client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
		client.DefaultRequestHeaders.Add("Accept-Language", "cs-CZ,cs;q=0.9");
	}
}
