using System.Collections.Concurrent;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public interface IControlContainer
{
    ConcurrentDictionary<string, IControl> Controls
    {
        get;
    }

    Task BuildControlsAsync(HttpContext context);

    Task<IControl> CreateControl<TControl>(
        HttpContext context,
        IControlContainer parent,
        IViewState view
    );

    Task PreRenderAsync(HttpContext context);
    Task ProcessPageAsync(HttpContext context);
    Task RenderAsync(HttpContext context, HttpResponseStreamWriter writer, HtmlEncoder htmlEncoder);
}
