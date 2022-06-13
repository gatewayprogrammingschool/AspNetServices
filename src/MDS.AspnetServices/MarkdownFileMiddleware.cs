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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var path = context.Request.Path;
            if(path.Value?.EndsWith("/mdapp/", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                await _next.Invoke(context);
                return;
            }
            if (path.Value?.EndsWith("/") ?? true)
            {
                path = path.Add(new PathString("/index.md"));
            }

            if (!(path.Value?.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase) ?? false))
            {
                await _next.Invoke(context);
                return;
            }

            var result = await Options.MarkdownFileExecute(context, path);
            await result.ExecuteAsync(context);

        }
        catch (Exception e)
        {
            await new MarkdownResponse(e)
                .ToMarkdownResult()
                .ExecuteAsync(context);
        }
    }
}