// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using Markdig.Syntax;

namespace MDS.AspnetServices.Common;

#pragma warning disable PH_S025 // Unused Synchronous Task Result
// ReSharper disable once ClassNeverInstantiated.Global
public record MarkdownResponse
{
    public static MarkdownPipeline Pipeline => _pipeline
        ??= MarkdownServerOptions.Current!.Services.GetService<MarkdownPipeline>()!;


    public MarkdownDocument Document { get; private set; }
    //private string? _layout = null;
    private static MarkdownPipeline? _pipeline;

    public MarkdownServerOptions? Options => MarkdownServerOptions.Current;
    public Exception? Error { get; }
    public HttpStatusCode StatusCode { get; }

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
        var rootPath = Document.GetData("root") as string;
        object? layout = null;
        variables?.TryGetValue("Variables.Layout", out layout);
        layout ??= Options?.Value.LayoutFile ?? "./wwwroot/DefaultLayout.html";
        
        if (rootPath is {  Length: > 0})
        {
            layout = Path.Combine(rootPath, layout!.ToString());
        }

        if (File.Exists(layout.ToString()))
        {
            layout = await File.ReadAllTextAsync(layout.ToString() ?? string.Empty);
        }
        else if(layout is null or "")
        {
            layout = "<html><head>$(title)</head><body>$(MarkdownBody)</body></html>";
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

        foreach ((var key, var value) in variables?.ToArray() ??
                                     Array.Empty<KeyValuePair<string, object>>())
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

        return template;
    }

    public MarkdownResult ToMarkdownResult()
        => new(Document);
}

#pragma warning restore PH_S025 // Unused Synchronous Task Result
