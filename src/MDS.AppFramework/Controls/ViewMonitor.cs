using Microsoft.AspNetCore.Components;

namespace MDS.AppFramework.Controls;

public record ViewMonitor(IViewWorkflow View, ILogger Logger) : IViewMonitor
{
    public CancellationToken Token { get; private set; }

    private readonly CancellationTokenSource _tokenSource = new();

    public event Func<IViewWorkflow, CancellationToken, Task>? ViewCompletedAsync;
    public event Func<IViewWorkflow, AggregateException, CancellationToken, Task>? ViewNotCompletedAsync;

    public string? Path { get; set; }

    public async Task StartAsync(HttpContext context, CancellationToken token = default)
    {
        Token = token;

        Token.Register(() => _tokenSource.Cancel());

        Task result = Task.CompletedTask;
        AggregateException? ae = new();
        try
        {
            Logger.LogInformation($"{GetType().Name}: Starting {View.GetType().Name}::{View.ViewKey}");
            context.Items.Add(nameof(View.ViewKey), View.ViewKey);
            result = View.StartAsync(context, _tokenSource.Token);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"{GetType().Name}: Handled {View.GetType().Name}::{View.ViewKey}{Environment.NewLine}{ex}");
            ae.InnerExceptions.Append(ex);
        }
        finally
        {
            Logger.LogInformation($"{GetType().Name}: Processed {View.GetType().Name}::{View.ViewKey}");
            if (!View.IsCompleted)
            {
                ae.InnerExceptions.Append(new ApplicationException("View did not complete processing."));
            }
        }

        if(ae.InnerExceptions.Count > 0)
        {
            result = Task.FromException(ae);

            if(ViewNotCompletedAsync is not null)
            {
                await ViewNotCompletedAsync(View, ae, token);
            }
        }

        if(ViewCompletedAsync is not null)
        {
            await ViewCompletedAsync(View, token);
        }

        await result;
    }

    public async Task StopAsync(CancellationToken token)
    {
        token.Register(() => _tokenSource.Cancel());

        Task result = Task.CompletedTask;
        AggregateException? ae = new();

        try{
            Logger.LogInformation($"{GetType().Name}: Stopping {View.GetType().Name}::{View.ViewKey}");
            _tokenSource.Cancel();

            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"{GetType().Name}: Handled {View.GetType().Name}::{View.ViewKey}{Environment.NewLine}{ex}");
            ae.InnerExceptions.Append(ex);
        }
        finally
        {
            Logger.LogInformation($"{GetType().Name}: Stopped {View.GetType().Name}::{View.ViewKey}");
            if (View.IsCompleted)
            {
                if(ViewCompletedAsync is not null)
                {
                    await ViewCompletedAsync(View, token);
                }
            }
            else
            {
                if(ViewNotCompletedAsync is not null)
                {
                    await ViewNotCompletedAsync(View, ae, token);
                }
            }
        }

        if(ae.InnerExceptions.Count > 0)
        {
            result = Task.FromException(ae);
        }

        await result;
    }
}