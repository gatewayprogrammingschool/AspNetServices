namespace MDS.AspnetServices.Common;

public class ContentResult : IResult
{
    public string Document
    {
        get;
    }

    /// <summary>Write an HTTP response reflecting the result.</summary>
    /// <param name="httpContext">The <see cref="T:Microsoft.AspNetCore.Http.HttpContext" /> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public Task ExecuteAsync(HttpContext context)
    {
        var memory = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(Document));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "text/plain";
        context.Response.ContentLength = memory.Length;

        return context.Response.BodyWriter.WriteAsync(memory)
            .AsTask();
    }

    public ContentResult()
    {
        Document = string.Empty;
    }

    public ContentResult(string document)
    {
        Document = document;
    }

    public ContentResult(byte[] document)
    {
        Document = Convert.ToBase64String(document);
    }
}
