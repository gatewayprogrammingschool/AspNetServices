using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public abstract record AppControlContainerBase(string Id) : AppControlBase(Id), IControlContainer
{
    public ConcurrentDictionary<string, IControl> Controls { get; } = new();
    
    public abstract Task BuildControlsAsync(HttpContext context);

    public override async Task InitAsync(HttpContext context)
    {
        foreach (var control in Controls)
        {
            await control.Value.InitAsync(context).ConfigureAwait(false);
        }
    }

    public override async Task InitializePageStateAsync(HttpContext context)
    {
        await context.Session.LoadAsync().ConfigureAwait(false);
        if (context.Session.TryGetValue(Id, out var serializedState))
        {
            var stream = new MemoryStream(serializedState);
            IViewState viewState = this;
            await viewState.DeserializePageStateAsync(context, stream).ConfigureAwait(false);
        }
        else
        {
            ViewState = new();
        }
    }

    public override async Task PreRenderAsync(HttpContext context)
    {
        foreach (var control in Controls)
        {
            await control.Value.PreRenderAsync(context).ConfigureAwait(false);
        }
    }
    
    public override async Task RenderAsync(HttpContext context, HttpResponseStreamWriter writer, HtmlEncoder htmlEncoder)
    {
        foreach (var control in Controls)
        {
            await control.Value.RenderAsync(context, writer, HtmlEncoder.Default).ConfigureAwait(false);
        }
    }
    
    public override async Task ProcessPageAsync(HttpContext context)
    {
        foreach (var control in Controls)
        {
            await control.Value.ProcessPageAsync(context).ConfigureAwait(false);
        }
    }

    public Task<IControl> CreateControl<TControl>(HttpContext context, IControlContainer parent, IViewState view)
    {
        throw new NotImplementedException();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public override async ValueTask DisposeAsync()
    {
        foreach (var disposable in Controls.Values)
        {
            await disposable.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}