global using MDS.AspnetServices;

var builder = WebApplication.CreateBuilder(args);
builder.AddMarkdownServer();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseMarkdownServer();

app.UseStaticFiles();

app.Run();
