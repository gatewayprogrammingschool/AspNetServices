global using MDS.AspnetServices;
using MDS.AppFramework.Common;
using MDS.TestSite.MdApp.Controllers;
using MDS.TestSite.MdApp.Views;

public class BlazorProgram
{
    private static WebApplication? _webApp = null;
    public static WebApplication WebApp => _webApp ??= BuildWebApp();


    public static async Task Main(string[] args)
    {
        _webApp = BuildWebApp();
        await StartWebApp(WebApp);
    }

    public static WebApplication BuildWebApp(params string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.AddMarkdownApplication(
            map =>
            {
                map.MapPathController<IndexController>("/mdapp/");
                map.MapPathController<IndexController>("/mdapp/index");
                map.MapPathController<IndexController>(
                    "/mdapp/index/search",
                    nameof(IndexController.Search)
                );

                builder.Services.AddScoped<IndexController>(
                    provider => new(
                        provider,
                        Guid.NewGuid()
                            .ToString()
                    )
                );
                builder.Services.AddScoped<IndexViewMonitor>();
            }
        );
        builder.AddMarkdownServer();

        builder.Services.AddHttpContextAccessor();

        return builder.Build();
    }

    public static Task StartWebApp(WebApplication webApp)
    {
        WebApp.UseMarkdownApplication();
        WebApp.UseMarkdownServer();

        WebApp.UseStaticFiles();

        return WebApp.RunAsync();
    }
}