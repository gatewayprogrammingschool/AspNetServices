using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Encodings.Web;

using MDS.AppFramework.Common;

using Microsoft.AspNetCore.WebUtilities;

namespace MDS.AppFramework.Controls;

public abstract record AppControlContainerBase(string Id) : AppControlBase(Id), IControlContainer
{
    public ConcurrentDictionary<string, IControl> Controls
    {
        get;
    } = new();

    public abstract Task BuildControlsAsync(HttpContext context);

    public Task<IControl> CreateControl<TControl>(
        HttpContext context,
        IControlContainer parent,
        IViewState view
    )
        => throw new NotImplementedException();

    public override async Task PreRenderAsync(HttpContext context)
    {
        foreach (KeyValuePair<string, IControl> control in Controls)
        {
            await control.Value.PreRenderAsync(context)
                .ConfigureAwait(false);
        }
    }

    public override async Task ProcessPageAsync(HttpContext context)
    {
        foreach (KeyValuePair<string, IControl> control in Controls)
        {
            await control.Value.ProcessPageAsync(context)
                .ConfigureAwait(false);
        }
    }

    public override async Task RenderAsync(
        HttpContext context,
        HttpResponseStreamWriter writer,
        HtmlEncoder htmlEncoder
    )
    {
        //writer.Indent++;

        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"<div id='{Id}'>");
        //writer.Indent++;
        await writer.WriteLineAsync(
            $"<form id='{Id}_form' action='{context.Request.Path}' method='POST'>"
        );
        //writer.Indent++;
        await writer.WriteLineAsync(
            $"<input type='hidden' value='{context.Items["ViewKey"]}' id='{Id}_ViewKey' name='{Id}|ViewKey' />"
        );

        foreach (KeyValuePair<string, IControl> control in Controls)
        {
            await control.Value.RenderAsync(context, writer, HtmlEncoder.Default)
                .ConfigureAwait(false);
        }


        await writer.WriteLineAsync(
            $"<button type='submit' value='submit' id='{Id}_Submit' name='{Id}|Submit'>Submit</button>"
        );
        //writer.Indent--;
        await writer.WriteLineAsync($"</form>");
        //writer.Indent--;
        await writer.WriteLineAsync($"</div>");
        await writer.WriteLineAsync();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public override async ValueTask DisposeAsync()
    {
        foreach (IControl? disposable in Controls.Values)
        {
            await disposable.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    public override async Task InitAsync(HttpContext context)
    {
        Logger?.LogInformation($"Initializing {Id}. IsPostBack={IsPostBack}");

        switch (IsPostBack)
        {
            case true:
                try
                {
                    PropertyInfo? pi = GetType()
                        .GetProperties()
                        .FirstOrDefault(
                            pi => pi.Name == "ViewModel" &&
                                  pi.PropertyType != typeof(ControlViewModel)
                        );
                    Type? bt = pi?.PropertyType.BaseType;
                    while (bt is not null
                           and
                        {
                            IsAbstract: false,
                        })
                        bt = bt.BaseType;

                    if (bt == typeof(ControlViewModel) &&
                        pi?.PropertyType is not null)
                    {
                        Task<ControlViewModel?>? viewModel
                            = context.GetViewModelAsync(this, pi.PropertyType);

                        if (viewModel is not null)
                        {
                            pi.SetValue(this, viewModel);
                            var lazy = await LazyContainer.CreateLazyContainerAsync(
                                () => viewModel,
                                viewModel
                            );
                            ViewState.AddOrUpdate(nameof(IAppView.ViewModel), lazy, (_, _) => lazy);
                        }
                    }
                }
                catch (Exception)
                {
                }

                break;
        }

        foreach (KeyValuePair<string, IControl> control in Controls)
        {
            await control.Value.InitAsync(context)
                .ConfigureAwait(false);
        }
    }

    public override async Task InitializePageStateAsync(HttpContext context)
    {
        await context.Session.LoadAsync()
            .ConfigureAwait(false);

        if (context.Session.TryGetValue(Id, out byte[]? serializedState))
        {
            MemoryStream stream = new(serializedState);
            IViewState viewState = this;
            await viewState.DeserializePageStateAsync(context, stream)
                .ConfigureAwait(false);
        }
        else
        {
            ViewState = new();
        }
    }
}
