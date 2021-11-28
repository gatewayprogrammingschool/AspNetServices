using System.Collections.Concurrent;
using System.Reflection;

using MDS.AppFramework.Controls;
using MDS.AspnetServices;
using MDS.AspnetServices.Common;

namespace MDS.AppFramework.Common
{
    internal class MarkdownApplicationMiddleware
    {
        private readonly RequestDelegate _next;
        public MarkdownApplicationOptions Options { get; }

        private ILogger<MarkdownApplicationMiddleware> _logger;

        public MarkdownApplicationMiddleware(RequestDelegate next, MarkdownApplicationOptions options)
        {
            _next = next;
            Options = options;
            _logger = Options.Services.GetRequiredService<ILogger<MarkdownApplicationMiddleware>>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var mre = new ManualResetEventSlim(false);

            try
            {
                var path = context.Request.Path;

                if (!(path.Value?.StartsWith("/mdapp/", StringComparison.InvariantCultureIgnoreCase) ?? false))
                {
                    await _next.Invoke(context);
                    return;
                }

                var map = Options.Services.GetRequiredService<ControllerMap>();

                Type? controllerType = map.GetControllerType(path);

                if (controllerType == null)
                {
                    _logger.LogWarning($"No controller found for {path}");
                    await _next.Invoke(context);
                    return;
                }

                var controller = AppController.BeginContext(controllerType, context, Options.Services);

                var viewMonitor = controller.GetViewMonitor(path);

                viewMonitor.ViewCompletedAsync -= ViewMonitor_ViewCompleted;
                viewMonitor.ViewCompletedAsync += ViewMonitor_ViewCompleted;

                viewMonitor.ViewNotCompletedAsync -= ViewMonitor_ViewNotCompleted;
                viewMonitor.ViewNotCompletedAsync += ViewMonitor_ViewNotCompleted;

                await viewMonitor.StartAsync(context, controller.CancellationTokenSource.Token);

                if (mre.Wait(TimeSpan.FromMinutes(5)))
                {
                    _logger.LogInformation($"Session {context.Session.Id} call to {path} completed successfully in {GetType().Name}.");
                    return;
                }

                _logger.LogWarning($"Session {context.Session.Id} call to {path} timed out in {GetType().Name}.");

                Task ViewMonitor_ViewNotCompleted(Controls.IViewWorkflow obj, AggregateException ae, CancellationToken token)
                {
                    if (ae is not null and { InnerExceptions.Count: > 0 })
                    {
                        _logger.LogError(ae, "Workflow returned exceptions.");
                        throw ae;
                    }

                    _logger.LogWarning($"Session {context.Session.Id} call to {path} did not complete.");

                    mre.Set();

                    return Task.CompletedTask;
                }

                async Task ViewMonitor_ViewCompleted(Controls.IViewWorkflow workflow, CancellationToken token)
                {
                    if (workflow.Exceptions?.InnerExceptions.Any() ?? false)
                    {
                        _logger.LogError(workflow.Exceptions, "Workflow returned exceptions.");
                        throw workflow.Exceptions;
                    }

                    var stream = workflow.Renderer;

                    if (stream is null)
                    {
                        throw new ApplicationException("Workflow provides no RenderStream.");
                    }

                    var response = workflow.StringBuilder.ToString();

                    // Get the Markdown file for the view
                    string viewName = workflow.GetType().FullName!;
                    var index = viewName.IndexOf(".mdapp.", StringComparison.OrdinalIgnoreCase);
                    if (index > -1)
                    {
                        index++;
                        var mdappName = viewName[index..].Replace(".", "\\") + ".md";

                        if (File.Exists(mdappName))
                        {
                            var filename = new FileInfo(mdappName).FullName;
                            ConcurrentDictionary<string, string> variables = new();
                            variables.TryAdd(nameof(workflow.ViewKey), workflow.ViewKey);
                            variables.TryAdd($"ViewBody:{workflow.ViewKey}", response);
                            variables.TryAdd(nameof(viewName), viewName);
                            if (workflow.ViewModel is not null)
                            {
                                ProcessViewModelAsync(workflow, variables);
                            }
                            var options = Options.Services.GetRequiredService<MarkdownServerOptions>();
                            var result = await options.MarkdownFileExecute(context, filename, variables);
                            await result.ExecuteAsync(context);
                        }
                    }


                    _logger.LogInformation($"Session {context.Session.Id} call to {path} completed.");

                    mre.Set();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{GetType().Name} handled exception.");
                await new MarkdownResponse(e)
                    .ToMarkdownResult()
                    .ExecuteAsync(context);
            }
        }

        private async Task ProcessViewModelAsync(IViewState viewState, ConcurrentDictionary<string, string> variables)
        {
            var lazy = viewState.ViewState.GetValueOrDefault(nameof(IAppView.ViewModel));
            var viewModel = lazy is not null ? await lazy.GetLazyDataAsync<ControlViewModel>() : null;

            if (viewModel is not null)
            {
                foreach (PropertyInfo pi in viewModel?.GetType().GetProperties(BindingFlags.Instance | System.Reflection.BindingFlags.Public) ?? Array.Empty<PropertyInfo>())
                {
                    var name = $"ViewModel.{pi.Name}";
                    var value = pi.GetValue(viewModel) as string ?? pi.GetValue(viewModel)?.ToString() ?? String.Empty;

                    variables.AddOrUpdate(name, value, (_, _) => value);
                }

                variables.AddOrUpdate("ViewModel", viewModel.ToString(), (_, _) => viewModel.ToString());
            }
        }

        public MarkdownApplicationConfiguration Value { get; } = new();

        public static MarkdownApplicationOptions? Current { get; private set; }
        private string? _serverRoot;

        public string? ServerRoot
        {
            get => _serverRoot;
            set => _serverRoot = value;
        }

        public IServiceProvider Services { get; set; }

    }
}