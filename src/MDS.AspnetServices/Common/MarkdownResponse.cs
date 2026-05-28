// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using Markdig.Syntax;
using Microsoft.Extensions.Hosting;

namespace MDS.AspnetServices.Common;

#pragma warning disable PH_S025 // Unused Synchronous Task Result
// ReSharper disable once ClassNeverInstantiated.Global
public record MarkdownResponse
{
    public static MarkdownPipeline Pipeline => _pipeline
        ??= MarkdownServerOptions.Current!.Services.GetService<MarkdownPipeline>()!;


    public MarkdownDocument Document
    {
        get; private set;
    }
    //private string? _layout = null;
    private static MarkdownPipeline? _pipeline;

    public MarkdownServerOptions? Options => MarkdownServerOptions.Current;
    public Exception? Error
    {
        get;
    }
    public HttpStatusCode StatusCode
    {
        get;
    }

    public MarkdownResponse()
    {
        StatusCode = HttpStatusCode.OK;
        Document = Markdown.Parse("# Index");
    }

    public MarkdownResponse(MarkdownDocument document)
    {
        Document = document;
        StatusCode = HttpStatusCode.OK;
    }

    private MarkdownResponse(string markdown, string? sidebarMarkdown) : this()
    {
        SetMarkdown(markdown);
        SetSidebarMarkdown(sidebarMarkdown);
    }

    public MarkdownResponse(HttpStatusCode statusCode) : this()
    {
        StatusCode = statusCode;
    }

    public MarkdownResponse(Exception error) : this()
    {
        Error = error;
        Document = Markdown.Parse($"---\nVariables:\n  Layout: ./wwwroot/error.html\n  Title: {error.Message}\n\n---\n\n# Error\n\n```\n{error}\n```\n");
        StatusCode = HttpStatusCode.InternalServerError;
    }

    private void SetMarkdown(string markdown)
        => Document = Markdown.Parse(markdown, Pipeline);

    private void SetSidebarMarkdown(string? markdown)
        => Document.SetData("SidebarContent", markdown ?? "");

    public static MarkdownResponse Create(MarkdownDocument document)
        => new(document);

    // ReSharper disable once UnusedMember.Global
    public static MarkdownResponse CreateFromFile(string filename)
        => new(File.ReadAllText(filename), "");

    public string ToHtml()
    {
        var html = Document.ToHtml(Pipeline);

        return html;
    }

    public async Task<byte[]> ToHtmlPage()
    {
        var html = ToHtml();

        var variables = Document.GetData("Variables") as ConcurrentDictionary<string, object>;
        var markdownRootPath = Document.GetData("root") as string;
        object? layout = null;
        variables?.TryGetValue("Variables.Layout", out layout);
        layout ??= Options?.Value.LayoutFile ?? "./wwwroot/DefaultLayout.html";

        // For layout files, use the application's content root, not the markdown root
        var contentRootPath = Options?.Services?.GetService<IHostEnvironment>()?.ContentRootPath ?? Directory.GetCurrentDirectory();

        if (contentRootPath is { Length: > 0 } && layout is string layoutPath && !Path.IsPathRooted(layoutPath))
        {
            layout = Path.Combine(contentRootPath, layoutPath);
        }

        if (File.Exists(layout.ToString()))
        {
            layout = await File.ReadAllTextAsync(layout.ToString() ?? string.Empty);
        }
        else
        {
            // Built-in GitHub-style fallback (used when no LayoutFile exists, e.g. `mds` on a folder without custom layout)
            // This provides GitHub Flavored Markdown *styling* as the default theming experience.
            layout = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>$(Variables.title)</title>
  <style>
    /* GitHub Markdown CSS - light theme approximation */
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
      font-size: 16px;
      line-height: 1.5;
      color: #24292f;
      background-color: #ffffff;
      padding: 40px;
      max-width: 900px;
      margin: 0 auto;
    }
    .markdown-body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
      font-size: 16px;
      line-height: 1.5;
      word-wrap: break-word;
    }
    .markdown-body h1, .markdown-body h2, .markdown-body h3, .markdown-body h4, .markdown-body h5, .markdown-body h6 {
      margin-top: 24px;
      margin-bottom: 16px;
      font-weight: 600;
      line-height: 1.25;
    }
    .markdown-body h1 { font-size: 2em; border-bottom: 1px solid #d0d7de; padding-bottom: .3em; }
    .markdown-body h2 { font-size: 1.5em; border-bottom: 1px solid #d0d7de; padding-bottom: .3em; }
    .markdown-body code, .markdown-body pre { font-family: ui-monospace, SFMono-Regular, 'SF Mono', Menlo, Consolas, 'Liberation Mono', monospace; }
    .markdown-body pre {
      padding: 16px;
      overflow: auto;
      font-size: 85%;
      line-height: 1.45;
      background-color: #f6f8fa;
      border-radius: 6px;
    }
    .markdown-body code { padding: .2em .4em; margin: 0; font-size: 85%; background-color: rgba(175,184,193,0.2); border-radius: 6px; }
    .markdown-body pre code { background-color: transparent; padding: 0; }

    /* GitHub-like syntax highlighting tokens (light) */
    .markdown-body .k { color: #d73a49; } /* keyword */
    .markdown-body .s { color: #032f62; } /* string */
    .markdown-body .c { color: #6a737d; } /* comment */
    .markdown-body .m { color: #005cc5; } /* number */
    .markdown-body .f { color: #6f42c1; } /* function */
    .markdown-body .o { color: #d73a49; } /* operator */
    .markdown-body table { border-spacing: 0; border-collapse: collapse; }
    .markdown-body table th, .markdown-body table td { padding: 6px 13px; border: 1px solid #d0d7de; }
    .markdown-body table tr { background-color: #ffffff; border-top: 1px solid #d0d7de; }
    .markdown-body table tr:nth-child(2n) { background-color: #f6f8fa; }
    .markdown-body blockquote { padding: 0 1em; color: #656d76; border-left: .25em solid #d0d7de; }
    .markdown-body a { color: #0969da; text-decoration: none; }
    .markdown-body a:hover { text-decoration: underline; }
    .markdown-body ul, .markdown-body ol { padding-left: 2em; }
    .markdown-body li + li { margin-top: .25em; }
  </style>
</head>
<body>
  <article class=""markdown-body"">
    $(MarkdownBody)
  </article>
</body>
</html>";
        }

        layout = await ReplaceVariables(variables, layout.ToString());

        if (layout is null)
        {
            return Array.Empty<byte>();
        }

        string page =
            layout.ToString()
                ?.Replace("$(MarkdownBody)", html) ?? "$(MarkdownBody)";

        page = await MarkdownProcessor.ProcessHtmlIncludes(page, variables);

        if (!Document.ContainsData("Variables"))
        {
            return page.ToUtf8Bytes();
        }

        page = await MarkdownResponse.ReplaceVariables(variables, page) ?? page;

        return page.ToUtf8Bytes();

    }

    private static async Task<string?> ReplaceVariables(ConcurrentDictionary<string, object>? variables, string? template)
    {
        if (template is null)
        {
            return null;
        }

        var vars = variables?.ToArray() ?? Array.Empty<KeyValuePair<string, object>>();

        foreach ((var key, var value) in vars)
        {
            switch (value)
            {
                case string toInsert:
                    {
                        if (toInsert.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (File.Exists(toInsert))
                            {
                                toInsert = await File.ReadAllTextAsync(toInsert);
                            }
                        }

                        template = template.Replace($"$({key})", toInsert);

                        break;
                    }

                default:
                    template = template.Replace($"$({key})", value.ToString());

                    break;
            }
        }

        // Convenience aliases for common variables (supports both $(title) and $(Variables.title) in layouts)
        if (variables != null)
        {
            if (variables.TryGetValue("Variables.title", out var vTitle) && !variables.ContainsKey("title"))
            {
                var titleStr = vTitle?.ToString() ?? "";
                template = template.Replace("$(title)", titleStr);
            }

            if (variables.TryGetValue("title", out var vTitle2) && !variables.ContainsKey("Variables.title"))
            {
                var titleStr = vTitle2?.ToString() ?? "";
                template = template.Replace("$(Variables.title)", titleStr);
            }
        }

        return template;
    }

    public MarkdownResult ToMarkdownResult()
        => new(Document);
}

#pragma warning restore PH_S025 // Unused Synchronous Task Result
