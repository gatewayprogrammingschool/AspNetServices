using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MDS.AspnetServices.Common;

namespace MDS.AppFramework.Common
{
    public static class MarkdownApplicationExtensions
    {
        public static WebApplicationBuilder AddMarkdownApplication(
            this WebApplicationBuilder builder,
            Action<ControllerMap>? addMappings = null)
        {
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();
            builder.Services
                .AddSingleton(provider
                    => builder.Configuration.GetSection("MarkdownServerApplication")
                        .Get<MarkdownApplicationConfiguration>())
                .AddSingleton<MarkdownApplicationOptions>()
                .AddSingleton(BuildControllerMapInstance(addMappings));

            builder.Services.AddSingleton<IViewMonitorManager, ViewMonitorManager>();

            return builder;

            ControllerMap BuildControllerMapInstance(Action<ControllerMap>? add = null)
            {
                ControllerMap map = new ();
                add?.Invoke(map);
                return map;
            }
        }

        public static WebApplication UseMarkdownApplication(this WebApplication app)
        {
            MarkdownApplicationConfiguration? config = app.Services.GetRequiredService<MarkdownApplicationConfiguration>();
            MarkdownApplicationOptions? options = app.Services.GetRequiredService<MarkdownApplicationOptions>();
            //new MarkdownServerOptions(app.Services, config);
            options.ServerRoot = app.Environment.WebRootPath;

            app.UseResponseCaching();
            app.UseSession();

            return (WebApplication)app.UseMiddleware<MarkdownApplicationMiddleware>();
        }

        internal static Task<IResult> MarkdownViewExecute(this MarkdownApplicationOptions options, HttpContext context, string viewPath)
        {
            string? fullPath = Path.Combine(options.ServerRoot, viewPath.TrimStart("\\/".ToCharArray()));
            Type? viewType = null;

            if(File.Exists(fullPath))
            {
                string? viewSource = File.ReadAllText(fullPath);

                // Get YAML frontmatter

                // Process ViewMonitor

                // Process Markdown

                // return result
            }
            else
            {
                viewType = Assembly.GetEntryAssembly()?.GetType(viewPath.Replace("/","_"));
            }

            if(viewType == null)
            {
                IResult notFound = Results.NotFound(viewPath);
                return Task.FromResult(notFound);
            }

            // Resolve View and execute
            IResult result = new MarkdownResult();
            return Task.FromResult(result);
        }
    }
}
