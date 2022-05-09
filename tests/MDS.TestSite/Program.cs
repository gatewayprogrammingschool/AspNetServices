global using MDS.AspnetServices;
using MDS.AppFramework;
using MDS.AppFramework.Common;
using MDS.TestSite.MdApp.Controllers;
using MDS.TestSite.MdApp.Views;

var builder = WebApplication.CreateBuilder(args);
//builder.AddMarkdownApplication(map =>
//{
//    map.MapPathController<IndexController>("/mdapp/");

//    builder.Services.AddScoped<IndexController>(provider => new IndexController(provider, Guid.NewGuid().ToString()));
//    builder.Services.AddScoped<IndexViewMonitor>();
//});
builder.AddMarkdownServer();

//builder.Services.AddHttpContextAccessor();

var app = builder.Build();

//app.UseMarkdownApplication();
app.UseMarkdownServer();

app.UseStaticFiles();

app.Run();
