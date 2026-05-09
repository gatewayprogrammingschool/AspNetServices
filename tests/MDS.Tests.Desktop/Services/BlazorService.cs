using MDS.Tests.Desktop.Core.Contracts.Services;

namespace MDS.Tests.Desktop.Services;
public class BlazorService : IBlazorService
{
    private readonly WebApplication _webHost;

    public BlazorService()
    {
        _webHost = BlazorProgram.WebApp;
    }

    public Task Initialize()
    {
        string path = Path.GetDirectoryName(typeof(BlazorService).Assembly.Location!)!;
        _webHost.Environment.ContentRootPath = Path.Combine(path, "wwwroot");
        return BlazorProgram.StartWebApp(_webHost);
    }
}
