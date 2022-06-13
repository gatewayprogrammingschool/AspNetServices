using System.Collections.Concurrent;
using System.Collections.Generic;

using MDS.AppFramework.Common;
using MDS.AppFramework.Controls;

namespace MDS.AppFramework;

public abstract record AppController(IServiceProvider Services) : IDisposable, IAsyncDisposable
{
    private static ConcurrentDictionary<string, HttpContext> _contexts = new();
    private static ConcurrentDictionary<string, IServiceScope> _scopes = new();

    protected ConcurrentDictionary<int, Type> _views = new();
    protected ConcurrentDictionary<int, IViewMonitor> _monitors = new();
    private bool disposedValue;
    private bool _asyncDisposing;

    public CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public static AppController BeginContext(PathControllerMapItem controllermapItem, HttpContext context, IServiceProvider provider)
    {
        _contexts.AddOrUpdate(context.Session.Id, context, (_, _) => context);

        IServiceScope? scope = _scopes.GetOrAdd(context.Session.Id, provider.CreateScope());

        AppController controller = (AppController)scope.ServiceProvider.GetRequiredService(controllermapItem.ControllerType!);

        return controller;
    }

    public static void EndContext(Type controllerType, HttpContext context)
    {
        if (_scopes.TryGetValue(context.Session.Id, out IServiceScope? scope))
        {
            AppController controller = (AppController)scope.ServiceProvider.GetRequiredService(controllerType);

            controller.CancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(15));
        }
    }

    public static void EndSession(Type controllerType, HttpContext context)
    {
        if (_scopes.TryGetValue(context.Session.Id, out IServiceScope? scope))
        {
            AppController controller = (AppController)scope.ServiceProvider.GetRequiredService(controllerType);

            controller.CancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(15));

            controller.DisposeAsync().GetAwaiter().GetResult();
        }
    }

    public IViewMonitor GetViewMonitor(PathString path)
        => GetViewMonitor<IViewMonitor>(path);

    public abstract TViewMonitor GetViewMonitor<TViewMonitor>(PathString path)
            where TViewMonitor : IViewMonitor;

    public TViewMonitor RegisterView<TViewMonitor>(PathString path, string id)
        where TViewMonitor : IViewMonitor, new()
    {
        int hash = path.GetHashCode();

        if (!_views.TryAdd(hash, typeof(TViewMonitor)))
        {
            throw new ApplicationException("Duplicate view cannot be added to controller.  Ensure ViewKey is unique per instance.");
        }

        return CreateViewMonitor<TViewMonitor>(path, id);
    }

    protected virtual TViewMonitor CreateViewMonitor<TViewMonitor>(PathString path, string id)
        where TViewMonitor : IViewMonitor, new()
    {
        TViewMonitor? monitor = new TViewMonitor();

        if (monitor is IViewMonitor toAdd)
        {
            int hash = path.GetHashCode();

            if (_monitors.TryAdd(hash, toAdd))
            {
                return monitor;
            }
        }

        throw new ApplicationException("Could not create IViewMonitor instance.");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _monitors.Clear();
                _views.Clear();
            }
        }
    }

    public void Dispose()
    {
        if (!_asyncDisposing)
        {
            return;
        }

        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Dispose();

                await ((ServiceProvider)Services).DisposeAsync();
            }

            disposedValue = true;
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        _asyncDisposing = true;
        // Do not change this code. Put cleanup code in 'DisposeAsync(bool disposing)' method
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}