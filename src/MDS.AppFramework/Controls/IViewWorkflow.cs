using System.Text;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public interface IViewWorkflow : IAppView, IViewState, IAsyncDisposable
{
    bool IsCompleted
    {
        get;
    }

    AggregateException? Exceptions
    {
        get;
    }

    HttpResponseStreamWriter Renderer
    {
        get;
    }

    string Id
    {
        get;
    }

    string Name
    {
        get;
    }

    StringBuilder StringBuilder
    {
        get;
    }

    Task InitAsync(HttpContext context);
    Task LoadPageStateAsync(HttpContext context);
    Task PreInitAsync(HttpContext context);
    Task PreRenderAsync(HttpContext context);
    Task ProcessPageAsync(HttpContext context);

    Task StartAsync(HttpContext context, CancellationToken token);
}
