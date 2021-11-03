using MDS.AppFramework.Controls;

namespace MDS.AppFramework;

public interface IViewMonitor
{
    IViewWorkflow View {get;}

    string Path { get; }
    
    Task StopAsync(CancellationToken token);

    event Action<IViewWorkflow> ViewCompleted;
    event Action<IViewWorkflow> ViewNotCompleted;

    Task StartAsync(HttpContext context, CancellationToken token);
}