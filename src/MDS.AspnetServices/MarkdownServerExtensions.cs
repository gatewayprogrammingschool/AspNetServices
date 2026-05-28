using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using MDS.AspnetServices.Theme;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Markdig;

namespace MDS.AspnetServices;

public static class MarkdownServerExtensions
{
    public static WebApplicationBuilder AddMarkdownServer(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(
                provider => builder.Configuration.GetSection("MarkdownServer")
                    .Get<MarkdownServerConfiguration>()
            )
            .AddSingleton<MarkdownServerOptions>();

        // Default to GitHub Flavored Markdown (as per requirements for generated directory indexes
        // when index.md is missing, and as the general default theming experience).
        builder.Services.AddSingleton(
            new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()   // tables, etc.
                .UseTaskLists()            // GFM task lists: - [ ]
                .UseAutoLinks()            // GFM autolinks
                .UseSyntaxHighlighting()
                .Build()
        );

        builder.Services.AddHttpClient();

        return builder;
    }

    public static Task<IResult> MarkdownFileExecute(
        this WebApplication app,
        HttpContext context,
        string? filename = null
    )
        => ((Task<IResult>)(MarkdownServerOptions.Current?.MarkdownFileExecute(context, filename) ??
                            Task<IResult>.FromException(new ApplicationException())));

    public static async Task<IResult> MarkdownFileExecute(
        this MarkdownServerOptions options,
        HttpContext context,
        string? filename = null,
        ConcurrentDictionary<string, object>? vars = null
    )
    {
        var rootPath = options.ServerRoot ?? "./wwwroot";
        rootPath = rootPath.Replace('/', Path.DirectorySeparatorChar);
        rootPath = Path.GetFullPath(rootPath);
        filename ??= "index.md";
        filename = filename.Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
        string markdownFilename = Path.Combine(rootPath, filename);
        string? originalPath = context.Request.Path.Value ?? "";

        // Check if this is a directory request (no specific file requested)
        // Robust handling for subfolders without index.md (the mds tool case).
        // Derive the physical directory from the request path when it's a directory request,
        // rather than defaulting through "index.md" which mangles subfolder paths.
        bool isDirectoryRequest = originalPath.EndsWith("/") || filename == "/" || filename?.EndsWith("/") == true;
        if (isDirectoryRequest)
        {
            // Compute the target physical directory directly from the original request path + ServerRoot.
            string requestDir = originalPath.TrimEnd('/');
            if (string.IsNullOrEmpty(requestDir)) requestDir = ".";
            string physicalDir = Path.Combine(rootPath, requestDir.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            physicalDir = Path.GetFullPath(physicalDir);

            if (Directory.Exists(physicalDir))
            {
                var mdFiles = Directory.EnumerateFiles(physicalDir, "*.md", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileName)
                    .OrderBy(f => f)
                    .ToList();

                string indexContent;
                if (mdFiles.Any())
                {
                    indexContent = "# Directory Index\n\n" +
                        "Welcome to this directory! Here are the available markdown files:\n\n" +
                        string.Join("\n", mdFiles.Select(f => "- [" + Path.GetFileNameWithoutExtension(f) + "](" + f + ")"));
                }
                else
                {
                    indexContent = "# Directory Index\n\n" +
                        "This directory contains no markdown files.";
                }

                var pipeline = MarkdownResponse.Pipeline;
                var doc = Markdown.Parse(indexContent, pipeline);

                // Set the same "root" metadata that normal file processing sets, so relative
                // includes, layouts, and any custom link rendering work consistently for pages
                // reached via generated indexes in folders without index.md.
                var rootForDoc = options.ServerRoot?.Replace("wwwroot", "").TrimEnd("/\\".ToCharArray()) ?? ".";
                doc.SetData("root", rootForDoc);

                return new MarkdownResult(doc);
            }
        }

        // Fallback for the original (root-level) logic if the above didn't apply.
        if (!File.Exists(markdownFilename) && (originalPath.EndsWith("/") || filename == "/"))
        {
            string dirPath = Path.GetDirectoryName(markdownFilename) ?? rootPath;
            if (Directory.Exists(dirPath))
            {
                var mdFiles = Directory.EnumerateFiles(dirPath, "*.md", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileName)
                    .OrderBy(f => f)
                    .ToList();

                string indexContent = mdFiles.Any()
                    ? "# Directory Index\n\nWelcome to this directory! Here are the available markdown files:\n\n" +
                      string.Join("\n", mdFiles.Select(f => "- [" + Path.GetFileNameWithoutExtension(f) + "](" + f + ")"))
                    : "# Directory Index\n\nThis directory contains no markdown files.";

                var pipeline = MarkdownResponse.Pipeline;
                var doc = Markdown.Parse(indexContent, pipeline);
                return new MarkdownResult(doc);
            }
        }
        var matrix = (markdownFilename,
            markdownFilename?.EndsWith("md", StringComparison.InvariantCultureIgnoreCase));
        try
        {
            var result = matrix switch
            {
                (null, _) => new MarkdownResponse(HttpStatusCode.NotFound).ToMarkdownResult(),
                (_, true) => await options.ProcessMarkdownFile(markdownFilename!, vars),
                _ => options.ProcessFile(filename),
            };
            if (result is not MarkdownResult mr)
            {
                return result;
            }

            var root = options.ServerRoot.Replace("wwwroot", "").TrimEnd("/\\\\".ToCharArray()) ?? ".";
            mr.Document.SetData("root", root);

            var variables = mr.Document.GetData("Variables") as ConcurrentDictionary<string, object>;
            variables ??= new();

            if (context.Request.HasFormContentType)
            {
                foreach (var (name, value) in context.Request.Form)
                {
                    variables.AddOrUpdate(
                        name,
                        string.Join(",", value),
                        (_, _) => string.Join(",", value)
                    );
                }
            }

            if (context.Request.HasJsonContentType())
            {
                var body = new StreamReader(context.Request.BodyReader.AsStream()).ReadToEnd();
                variables.AddOrUpdate("Body", body, (_, _) => body);
            }

            foreach (var (name, value) in context.Request.Query)
            {
                variables.AddOrUpdate(name, string.Join(",", value), (_, _) => string.Join(",", value));
            }

            if (!variables.ContainsKey("Variables.title"))
            {
                var fallbackTitle = context.Request.Path.Value ?? "";
                variables.AddOrUpdate("Variables.title", fallbackTitle, (_, old) => old);
                // Also provide bare "title" for layouts that use the simpler $(title) syntax
                variables.AddOrUpdate("title", fallbackTitle, (_, old) => old);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new AggregateException("Exception caught while executing MarkdownFileExecute Middleware.", ex)
            {
                Data =
                {
                    { nameof(filename), filename },
                    { nameof(matrix), matrix },
                }
            };
        }
    }

    public static byte[] ToUtf8Bytes(this string toEncode)
        => Encoding.UTF8.GetBytes(toEncode);

    public static WebApplication UseMarkdownServer(this WebApplication app)
    {
        Console.WriteLine("UseMarkdownServer method called - DEBUG");
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("MarkdownServer");
        logger.LogInformation("UseMarkdownServer method called");

        var options = app.Services.GetRequiredService<MarkdownServerOptions>()!;
        options.ServerRoot = app.Environment.WebRootPath;

        // Log assembly version as soon as logger is initialized
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "unknown";
        logger.LogInformation("MarkdownServer assembly version: {Version}", version);
        Console.WriteLine($"MarkdownServer assembly version: {version} - DEBUG");

        return (WebApplication)app.UseMiddleware<MarkdownFileMiddleware>();
    }

    public static WebApplicationBuilder AddTheme(this WebApplicationBuilder builder, Action<ThemeOptions>? configure = null)
    {
        builder.Services.Configure<ThemeOptions>(builder.Configuration.GetSection("Theme"));
        builder.Services.AddSingleton<ThemeManager>();
        var sp = builder.Services.BuildServiceProvider();
        configure?.Invoke(sp.GetRequiredService<IOptions<ThemeOptions>>().Value);
        return builder;
    }
}
