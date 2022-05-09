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

    public async Task<byte[]> ToHtmlPage()
    {
        var html = ToHtml();

        var variables = Document.GetData("Variables") as ConcurrentDictionary<string, string>;

        if(!(variables?.TryGetValue("Layout", out var layout) ?? true))
        {
            layout = _layout ??= await File.ReadAllTextAsync(Options?.Value.LayoutFile ?? "./wwwroot/DefaultLayout.html");
        }
        else
        {
            layout = _layout;
        }

        var page =
            layout?.Replace("$(MarkdownBody)", html) ?? "$(MarkdownBody)";

        page = await MarkdownProcessor.ProcessHtmlIncludes(page, variables);

        if (!Document.ContainsData("Variables"))
        {
            return page.ToUtf8Bytes();
        }

        foreach (var (key, value) in variables?.ToArray() ?? Array.Empty<KeyValuePair<string,string>>())
        {
            var toInsert = value;
            if (value?.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                if (File.Exists(value))
                {
                     toInsert = await File.ReadAllTextAsync(value);
                }
            }

            page = page.Replace($"$({key})", toInsert);
        }

        return page.ToUtf8Bytes();
    }

    public MarkdownResult ToMarkdownResult()
    {
        return new MarkdownResult(Document);
    }
}

#pragma warning restore PH_S025 // Unused Synchronous Task Result
