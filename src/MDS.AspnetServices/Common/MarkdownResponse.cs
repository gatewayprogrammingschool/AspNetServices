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
    private string? _layout = null;
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

    private MarkdownResponse(string markdown, string? sidebarMarkdown, MarkdownServerOptions? options = null) : this()
    {
        SetMarkdown(markdown);
        SetSidebarMarkdown(sidebarMarkdown);
    }

    public MarkdownResponse(HttpStatusCode statusCode, MarkdownServerOptions? options = null) : this()
    {
        StatusCode = statusCode;
    }

    public MarkdownResponse(Exception error, MarkdownServerOptions? options = null) : this()
    {
        Error = error;
        StatusCode = HttpStatusCode.InternalServerError;
    }

    private void SetMarkdown(string markdown)
    {
        Document = Markdown.Parse(markdown, Pipeline);
    }

    private void SetSidebarMarkdown(string? markdown)
    {
        Document.SetData("SidebarContent", markdown ?? "");
    }

    public static MarkdownResponse Create(MarkdownDocument document)
        => new(document);

    public static MarkdownResponse CreateFromFile(string filename)
        => new(File.ReadAllText(filename), "");

    public string ToHtml()
    {
        var html = Document.ToHtml(Pipeline);

        return html;
    }

    public async Task<string> ToHtmlPage()
    {
        var html = ToHtml();

        var layout = _layout ??= File.ReadAllText(Options?.Value.LayoutFile ?? "./wwwroot/DefaultLayout.html");
        
        var page =
            layout.Replace("$(MarkdownBody)", html);

        page = await MarkdownProcessor.ProcessHtmlIncludes(page);

        if (!Document.ContainsData("Variables"))
        {
            return page;
        }

        if (Document.GetData("Variables") is not ConcurrentDictionary<string, string> variables)
        {
            return page;
        }

        foreach (var (key, value) in variables)
        {
            var toInsert = value;
            if (value.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase))
            {
                if (File.Exists(value))
                {
                     toInsert = File.ReadAllText(value);
                }
            }

            page = page.Replace($"$({key})", toInsert);
        }

        return page;
    }

    public MarkdownResult ToMarkdownResult()
    {
        return new MarkdownResult(Document);
    }
}

#pragma warning restore PH_S025 // Unused Synchronous Task Result
