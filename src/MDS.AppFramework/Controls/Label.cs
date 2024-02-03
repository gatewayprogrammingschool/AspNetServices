using System.Collections.Concurrent;
using System.Text.Encodings.Web;

using MDS.AppFramework.Common;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public class Label : IControl
{
    public string Id
    {
        get;
        set;
    } = null!;

    public string? Name
    {
        get;
        set;
    }

    public bool IsPostBack
    {
        get;
    }

    public IViewState? Parent
    {
        get;
        set;
    }

    public ILogger? Logger
    {
        get;
        set;
    }

    public ConcurrentDictionary<string, LazyContainer> ViewState
    {
        get;
        set;
    } = null!;

    public ValueTask DisposeAsync()
        => throw new NotImplementedException();

    public Task InitAsync(HttpContext context)
        => throw new NotImplementedException();

    public Task PreRenderAsync(HttpContext context)
        => throw new NotImplementedException();

    public Task ProcessPageAsync(HttpContext context)
        => throw new NotImplementedException();

    public Task RenderAsync(
        HttpContext context,
        HttpResponseStreamWriter writer,
        HtmlEncoder htmlEncoder
    )
        => throw new NotImplementedException();

    public Task InitializePageStateAsync(HttpContext context)
        => throw new NotImplementedException();
}
