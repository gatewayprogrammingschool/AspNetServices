namespace MDS.AspnetServices;

public class MarkdownFileMiddleware
{
    private readonly RequestDelegate _next;
    public MarkdownServerOptions Options { get; }

    public MarkdownFileMiddleware(RequestDelegate next, MarkdownServerOptions options)
    {
        _next = next;
        Options = options;
    }

    public Task InvokeAsync(HttpContext context)
    {
        try
        {
            var path = context.Request.Path;
            if (path.Value?.EndsWith("/") ?? true)
            {
                path = path.Add(new PathString("/index.md"));
            }

            if (!(path.Value?.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase) ?? false))
            {
                return _next.Invoke(context);
            }

            var result = Options.MarkdownFileExecute(context, path);
            return result.ExecuteAsync(context);

        }
        catch (Exception e)
        {
            return new MarkdownResponse(e)
                .ToMarkdownResult()
                .ExecuteAsync(context);
        }
    }
}