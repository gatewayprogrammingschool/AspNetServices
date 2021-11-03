using Microsoft.AspNetCore.Components;

namespace MDS.AppFramework.Controls;

public record ViewMonitor(IViewWorkflow View, ILogger Logger) : IViewMonitor
{
    public CancellationToken Token { get; private set; }

    private readonly CancellationTokenSource _tokenSource = new();

    public event Action<IViewWorkflow>? ViewCompleted;
    public event Action<IViewWorkflow>? ViewNotCompleted;

    public string Path { get; set; }

    public Task StartAsync(HttpContext context, CancellationToken token = default)
    {
        Token = token;

        Token.Register(() => _tokenSource.Cancel());
        
        Task result = Task.CompletedTask;
        var ae = new AggregateException();
        try
        {
            Logger.LogInformation($"{GetType().Name}: Starting {View.GetType().Name}::{View.ViewKey}");
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
            ViewNotCompleted?.Invoke(View);
        }

        ViewCompleted?.Invoke(View);
        return result;
    }

    public Task StopAsync(CancellationToken token)
    {
        token.Register(() => _tokenSource.Cancel());

        Task result = Task.CompletedTask;
        var ae = new AggregateException();

        try{
            Logger.LogInformation($"{GetType().Name}: Stopping {View.GetType().Name}::{View.ViewKey}");
            _tokenSource.Cancel();

            return Task.CompletedTask;
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
                try
                {
                    ViewCompleted?.Invoke(View);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{GetType().Name}: Handled {View.GetType().Name}::{View.ViewKey}{Environment.NewLine}{ex}");
                    ae.InnerExceptions.Append(ex);
                }
            }
            else
            {
                try
                {
                    ViewNotCompleted?.Invoke(View);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"{GetType().Name}: Handled {View.GetType().Name}::{View.ViewKey}{Environment.NewLine}{ex}");
                    ae.InnerExceptions.Append(ex);
                }
            }
        }

        if(ae.InnerExceptions.Count > 0)
        {
            result = Task.FromException(ae);
        }

        return result;
    }
}