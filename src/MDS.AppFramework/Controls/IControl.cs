using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public interface IControl : IViewState, IAsyncDisposable
{
    string Id { get; set; }
    string? Name { get; set; }
    IViewState? Parent { get; set; }

    Task InitAsync(HttpContext context);
    Task ProcessPageAsync(HttpContext context);
    Task PreRenderAsync(HttpContext context);
    
    Task RenderAsync(HttpContext context, HttpResponseStreamWriter writer, HtmlEncoder htmlEncoder);
}

public interface IControlWithModel<TViewModel> : IControl
    where TViewModel : ControlViewModel
{

}