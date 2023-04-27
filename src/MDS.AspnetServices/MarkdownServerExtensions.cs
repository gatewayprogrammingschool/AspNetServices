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

        builder.Services.AddSingleton(
            new MarkdownPipelineBuilder().UseAdvancedExtensions()
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
        rootPath = rootPath.Replace("/", "\\");
        rootPath = Path.GetFullPath(rootPath);
        filename ??= "index.md";
        filename = filename.Replace("/", "\\")
            .TrimStart('\\');
        string markdownFilename = Path.Combine(rootPath, filename);

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
                variables.AddOrUpdate("Body", body, (_, _) => string.Join(",", body));
            }

            foreach (var (name, value) in context.Request.Query)
            {
                variables.AddOrUpdate(name, string.Join(",", value), (_, _) => string.Join(",", value));
            }

            if (!variables.ContainsKey("title"))
            {
                variables.AddOrUpdate("title", context.Request.Path, (_, old) => old);
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
        var options = app.Services.GetRequiredService<MarkdownServerOptions>()!;
        options.ServerRoot = app.Environment.WebRootPath;

        return (WebApplication)app.UseMiddleware<MarkdownFileMiddleware>();
    }
}
