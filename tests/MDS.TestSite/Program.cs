global using MDS.AspnetServices;
using MDS.AppFramework;
using MDS.AppFramework.Common;
using MDS.TestSite.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.AddMarkdownServer();
builder.AddMarkdownApplication(map =>
{
    map.MapPathController<IndexController>("/index.mdapp");
 
    builder.Services.AddScoped<IndexController>(provider => new IndexController(provider, Guid.NewGuid().ToString()));
    builder.Services.AddScoped<IndexViewMonitor>();
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseMarkdownServer();
app.UseMarkdownApplication();

app.UseStaticFiles();

app.Run();
