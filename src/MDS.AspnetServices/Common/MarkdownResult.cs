namespace MDS.AspnetServices.Common;

public class MarkdownResult : IResult
{
    public string Markdown { get; set; }
    public string? SidebarMarkdown { get; set; }

    public MarkdownResult()
    {
        Markdown = "";
        SidebarMarkdown = "";
    }

    public MarkdownResult(string markdown, string? requestSidebarContents)
    {
        Markdown = markdown;
        SidebarMarkdown = requestSidebarContents;
    }

    /// <summary>Write an HTTP response reflecting the result.</summary>
    /// <param name="httpContext">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public Task ExecuteAsync(HttpContext context)
    {
        var html = MarkdownResponse.Create(Markdown, SidebarMarkdown).ToHtmlPage();
        var memory = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(html));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "text/html";
        context.Response.ContentLength = html.Length;
        return context.Response.BodyWriter.WriteAsync(memory).AsTask();
    }
}