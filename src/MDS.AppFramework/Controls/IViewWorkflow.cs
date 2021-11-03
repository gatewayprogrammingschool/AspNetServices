namespace MDS.AppFramework.Controls;

public interface IViewWorkflow : IAppView, IViewState, IAsyncDisposable
{
    bool IsPostBack { get; }
    bool IsCompleted {get;}

    AggregateException? Exceptions { get; }

    Task StartAsync(HttpContext context, CancellationToken token);
    Task PreInitAsync(HttpContext context);
    Task InitAsync(HttpContext context);
    Task LoadPageStateAsync(HttpContext context);
    Task ProcessPageAsync(HttpContext context);
    Task PreRenderAsync(HttpContext context);
}