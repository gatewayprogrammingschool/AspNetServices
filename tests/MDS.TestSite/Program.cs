global using MDS.AspnetServices;

var builder = WebApplication.CreateBuilder(args);
builder.AddMarkdownServer();

builder.Services.AddHttpContextAccessor();
//builder.Services.AddOptions();

var app = builder.Build();

app.UseMarkdownServer();

app.UseStaticFiles();

//app.MapGet("/", async context 
//    => await MarkdownServerOptions
//        .Current
//        .MarkdownFileExecute(context)
//        .ExecuteAsync(context));

app.Run();
