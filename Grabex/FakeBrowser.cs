namespace Grabex;

public abstract class FakeBrowser : IDisposable
{
	protected readonly HttpClient client;

	public FakeBrowser()
	{
		HttpClientHandler handler = new();
		client = new HttpClient(handler);
	}

	public abstract HttpClient GetBrowser();
	
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	// Protected implementation of Dispose pattern.
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			client.Dispose();
		}
	}

	~FakeBrowser()
	{
		Dispose(false);
	}
}
