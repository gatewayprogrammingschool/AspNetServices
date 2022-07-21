using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using MDS.AppFramework.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public abstract record AppControlBase(string Id) : IControl
{
    public bool IsPostBack { get; internal set; }
    public ILogger? Logger {get;set;}

    public ConcurrentDictionary<string, LazyContainer> ViewState { get; set; } = new();

    public virtual async Task InitializePageStateAsync(HttpContext context)
    {
        LazyContainer? id = await LazyContainer.CreateLazyContainerAsync(() => Id, Id);
        ViewState.AddOrUpdate(nameof(Id), id, (_, existing) => id);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public virtual async ValueTask DisposeAsync()
    {
        foreach (IAsyncDisposable disposable in 
                 ViewState.Values.Select(async l 
                         => await l.GetLazyDataAsync<object>())
                     .OfType<IAsyncDisposable>())
        {
            await disposable.DisposeAsync();
        }
    }

    
    public IViewState? Parent { get; set; }

    public string Name { get; set; } = Id;

    public virtual Task InitAsync(HttpContext context)
        => Task.CompletedTask;

    public virtual Task ProcessPageAsync(HttpContext context)
        => Task.CompletedTask;

    public virtual Task PreRenderAsync(HttpContext context)
        => Task.CompletedTask;

    public abstract Task RenderAsync(HttpContext context, HttpResponseStreamWriter writer, HtmlEncoder htmlEncoder);
}