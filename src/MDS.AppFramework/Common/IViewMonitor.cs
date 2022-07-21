using MDS.AppFramework.Controls;

namespace MDS.AppFramework;

public interface IViewMonitor
{
    IViewWorkflow View {get;}

    string? Path { get; }

    Task StopAsync(CancellationToken token);

    event Func<IViewWorkflow, CancellationToken, Task> ViewCompletedAsync;
    event Func<IViewWorkflow, AggregateException, CancellationToken, Task> ViewNotCompletedAsync;

    Task StartAsync(HttpContext context, CancellationToken token);
}