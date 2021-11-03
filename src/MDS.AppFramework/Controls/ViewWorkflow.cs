using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;

using MDS.AppFramework.Common;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public abstract record ViewWorkflow(string Id) : AppControlContainerBase(Id), IViewWorkflow
{
    public string ViewKey => Id;

    public bool IsPostBack { get; private set; }
    public bool IsCompleted { get; }

    public ControlViewModel? ViewModel {get;set;}

    public AggregateException? Exceptions { get; private set; }

    private bool _isDisposed = false;
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
            await BuildControlsAsync(context).ConfigureAwait(false);
            await InitializePageStateAsync(context).ConfigureAwait(false);
            await InitAsync(context).ConfigureAwait(false);

            await LoadPageStateAsync(context).ConfigureAwait(false);
            await ProcessPageAsync(context).ConfigureAwait(false);

            await PreRenderAsync(context).ConfigureAwait(false);

            var writer = new HttpResponseStreamWriter(context.Response.BodyWriter.AsStream(true), Encoding.UTF8);
            await RenderAsync(context, writer, HtmlEncoder.Default).ConfigureAwait(false);
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
            Id = context.Request.Form[nameof(ViewKey)];
            IsPostBack = true;
        }
        else
        {
            Id = Guid.NewGuid().ToString();
            IsPostBack = false;
        }

        return InitAsync(context);
    }

}