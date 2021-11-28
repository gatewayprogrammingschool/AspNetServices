using System.CodeDom.Compiler;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public interface IControl : IViewState, IAsyncDisposable
{
    string Id { get; }
    string? Name { get; }
    bool IsPostBack { get; }
    IViewState? Parent { get; set; }
    ILogger? Logger {get;set;}

    Task InitAsync(HttpContext context);
    Task ProcessPageAsync(HttpContext context);
    Task PreRenderAsync(HttpContext context);
    
    Task RenderAsync(HttpContext context, HttpResponseStreamWriter writer, HtmlEncoder htmlEncoder);
}

public interface IControlWithModel<TViewModel> : IControl
    where TViewModel : ControlViewModel
{

}