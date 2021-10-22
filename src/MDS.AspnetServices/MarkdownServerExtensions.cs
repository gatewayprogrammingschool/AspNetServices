using Microsoft.Extensions.Options;

namespace MDS.AspnetServices;

public static class MarkdownServerExtensions
{
    public static WebApplicationBuilder AddMarkdownServer(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(provider 
            => builder.Configuration.GetSection("MarkdownServer")
                .Get<MarkdownServerConfiguration>());
        
        return builder;
    }

    public static WebApplication UseMarkdownServer(this WebApplication app)
    {
        var config = app.Services.GetRequiredService<MarkdownServerConfiguration>();
        var options = new MarkdownServerOptions(config);
        return app.UseMarkdownServer(options);
    }

    public static WebApplication UseMarkdownServer(this WebApplication app, MarkdownServerOptions options)
    {
        MarkdownServerOptions.Current = options;
        return app;
    }
}