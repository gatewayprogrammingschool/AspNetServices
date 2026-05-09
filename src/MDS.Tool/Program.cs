using MDS.AspnetServices;

if (args.Contains("--help"))
{
    Console.WriteLine("Usage: mds [--port <port>] [--root <path>]");
    Console.WriteLine("  --port <port>  Port to listen on (default 5000)");
    Console.WriteLine("  --root <path>  Root directory (default current)");
    return;
}

var rootDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var port = 5000;
var wwwroot = Path.Combine(rootDirectory, "wwwroot");

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--port" when i + 1 < args.Length:
            port = int.Parse(args[++i]);
            break;
        case "--root" when i + 1 < args.Length:
            wwwroot = args[++i];
            break;
    }
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = rootDirectory,
    WebRootPath = wwwroot
});

builder.WebHost.UseUrls($"http://localhost:{port}");

builder.AddMarkdownServer();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMarkdownServer();

Console.WriteLine($"MarkdownServer starting on http://localhost:{port}");
Console.WriteLine($"Serving content from: {wwwroot}");

app.Run();