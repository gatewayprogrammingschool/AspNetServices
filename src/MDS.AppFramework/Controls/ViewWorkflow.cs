using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;

using MDS.AppFramework.Common;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public abstract record ViewWorkflow(string Id) : AppControlContainerBase(Id), IViewWorkflow
{
    public string ViewKey {get;set; } = Guid.NewGuid().ToString();

    public bool IsCompleted { get; }

    public virtual ControlViewModel? ViewModel {get;set;}

    public AggregateException? Exceptions { get; private set; }

    public StringBuilder StringBuilder { get; } = new StringBuilder();
    public IndentedTextWriter Renderer => _renderer ??= 
        new IndentedTextWriter(new StringWriter(StringBuilder), "\t");

    private bool _isDisposed = false;
    private IndentedTextWriter _renderer;

    public override async ValueTask DisposeAsync()
    {
        if(!_isDisposed)
        { 
            try
            {
                await base.DisposeAsync();
            }
            catch (Exception ex)
            {
                UpdateExceptions(ex);
            }

            ViewState?.Clear();

            _isDisposed = true;
        }
    }

    public async Task StartAsync(HttpContext context, CancellationToken token)
    {
        try
        {
            Logger = MarkdownApplicationOptions.Current!.Services.GetRequiredService<ILogger<ViewWorkflow>>();

            if(context.Request.Method == HttpMethod.Post.ToString())
            {
                IsPostBack = true;
                ViewKey = context.Request.Form[$"{Id}_ViewKey"];
            }

            await BuildControlsAsync(context).ConfigureAwait(false);
            await InitializePageStateAsync(context).ConfigureAwait(false);
            await InitAsync(context).ConfigureAwait(false);

            await LoadPageStateAsync(context).ConfigureAwait(false);
            await ProcessPageAsync(context).ConfigureAwait(false);

            await PreRenderAsync(context).ConfigureAwait(false);

            await RenderAsync(context, Renderer, HtmlEncoder.Default).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateExceptions(ex);
        }
    }

    private void UpdateExceptions(Exception ex)
    {
        if (Exceptions is null)
        {
            Exceptions = new AggregateException(ex);
        }
        else
        {
            Exceptions.InnerExceptions.Append(ex);
        }
    }

    public virtual Task LoadPageStateAsync(HttpContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task PreInitAsync(HttpContext context)
    {
        if (context.Request.HasFormContentType)
        {
            ViewKey = context.Request.Form[nameof(ViewKey)];
            IsPostBack = true;
        }
        else
        {
            ViewKey = Guid.NewGuid().ToString();
            IsPostBack = false;
        }

        return InitAsync(context);
    }

}