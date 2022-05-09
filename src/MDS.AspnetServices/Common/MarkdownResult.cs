using Markdig.Syntax;

namespace MDS.AspnetServices.Common;

public class MarkdownResult : IResult
{
    public MarkdownDocument Document { get; }
    public MarkdownResult()
    {
        Document = new MarkdownDocument();
    }

    public MarkdownResult(MarkdownDocument document)
    {
        Document = document;
    }

    /// <summary>Write an HTTP response reflecting the result.</summary>
    /// <param name="httpContext">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext context)
    {
        var html = await MarkdownResponse.Create(Document).ToHtmlPage();
        var memory = new ReadOnlyMemory<byte>(html);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "text/html";
        context.Response.ContentLength = memory.Length;
        await context.Response.BodyWriter.WriteAsync(memory).AsTask();
    }
}