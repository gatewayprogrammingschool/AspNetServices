using System.Text;
using System.Text.Encodings.Web;

using MDS.AppFramework.Common;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public abstract record ViewWorkflow(string Id) : AppControlContainerBase(Id), IViewWorkflow
{
    public string ViewKey
    {
        get;
        set;
    } = Guid.NewGuid()
        .ToString();

    public bool IsCompleted
    {
        get;
    }

    public ControlViewModel? ViewModel
    {
        get => ViewState.TryGetValue(nameof(ViewModel), out LazyContainer? value)
            ? value.GetLazyDataAsync<ControlViewModel>()
                .GetAwaiter()
                .GetResult()
            : default;
        set => ViewState.AddOrUpdate(
            nameof(ViewModel),
            MakeLazy(value!)
                .GetAwaiter()
                .GetResult(),
            (_, _) => MakeLazy(value!)
                .GetAwaiter()
                .GetResult()
        );
    }

    public AggregateException? Exceptions
    {
        get;
        private set;
    }

    public StringBuilder StringBuilder
    {
        get;
    } = new();

    public HttpResponseStreamWriter Renderer => _renderer ??= new(
        Context?.Response.BodyWriter.AsStream() ?? throw new NullReferenceException(),
        Encoding.UTF8
    );

    public HttpContext? Context
    {
        get;
        private set;
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_isDisposed)
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

    public virtual Task LoadPageStateAsync(HttpContext context)
        => Task.CompletedTask;

    public virtual Task PreInitAsync(HttpContext context)
    {
        if (context.Request.HasFormContentType)
        {
            ViewKey = context.Request.Form[nameof(ViewKey)];
            IsPostBack = true;
        }
        else
        {
            ViewKey = Guid.NewGuid()
                .ToString();
            IsPostBack = false;
        }

        return InitAsync(context);
    }

    public async Task StartAsync(HttpContext context, CancellationToken token)
    {
        try
        {
            Logger = MarkdownApplicationOptions.Current!.Services
                .GetRequiredService<ILogger<ViewWorkflow>>();

            if (context.Request.Method == HttpMethod.Post.ToString())
            {
                IsPostBack = true;
                ViewKey = context.Request.Form[$"{Id}_ViewKey"];
            }

            Context = context;

            await BuildControlsAsync(context)
                .ConfigureAwait(false);
            await InitializePageStateAsync(context)
                .ConfigureAwait(false);
            await InitAsync(context)
                .ConfigureAwait(false);

            await LoadPageStateAsync(context)
                .ConfigureAwait(false);
            await ProcessPageAsync(context)
                .ConfigureAwait(false);

            await PreRenderAsync(context)
                .ConfigureAwait(false);

            await RenderAsync(context, Renderer, HtmlEncoder.Default)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateExceptions(ex);
        }
    }

    private bool _isDisposed = false;
    private HttpResponseStreamWriter? _renderer;

    private Task<LazyContainer> MakeLazy(ControlViewModel viewModel)
    {
        Func<ControlViewModel> func = () => viewModel;

        return LazyContainer.CreateLazyContainerAsync<ControlViewModel>(func, viewModel);
    }

    private void UpdateExceptions(Exception ex)
    {
        if (Exceptions is null)
        {
            Exceptions = new(ex);
        }
        else
        {
            Exceptions.InnerExceptions.Append(ex);
        }
    }
}
