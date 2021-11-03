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

                if (!(path.Value?.EndsWith(".mdapp", StringComparison.InvariantCultureIgnoreCase) ?? false))
                {
                    await _next.Invoke(context);
                    return;
                }

                var map = Options.Services.GetRequiredService<ControllerMap>();

                Type? controllerType = map.GetControllerType(path);

                if(controllerType == null)
                {
                    _logger.LogWarning($"No controller found for {path}");
                    await _next.Invoke(context);
                    return;
                }

                var controller = AppController.BeginContext(controllerType, context, Options.Services);

                var viewMonitor = controller.GetViewMonitor(path);

                viewMonitor.ViewCompleted -= ViewMonitor_ViewCompleted;
                viewMonitor.ViewCompleted += ViewMonitor_ViewCompleted;

                viewMonitor.ViewNotCompleted -= ViewMonitor_ViewNotCompleted;
                viewMonitor.ViewNotCompleted += ViewMonitor_ViewNotCompleted;

                await viewMonitor.StartAsync(context, controller.CancellationTokenSource.Token);

                if(mre.Wait(TimeSpan.FromMinutes(5)))
                {
                    _logger.LogInformation($"Session {context.Session.Id} call to {path} completed successfully in {GetType().Name}.");
                    return;
                }

                _logger.LogWarning($"Session {context.Session.Id} call to {path} timed out in {GetType().Name}.");

                void ViewMonitor_ViewNotCompleted(Controls.IViewWorkflow obj)
                {
                    if(obj.Exceptions?.InnerExceptions.Any() ?? false)
                    {
                        _logger.LogError(obj.Exceptions, "Workflow returned exceptions.");
                        throw obj.Exceptions;
                    }

                    _logger.LogWarning($"Session {context.Session.Id} call to {path} did not complete.");

                    mre.Set();
                }

                void ViewMonitor_ViewCompleted(Controls.IViewWorkflow obj)
                {
                    if(obj.Exceptions?.InnerExceptions.Any() ?? false)
                    {
                        _logger.LogError(obj.Exceptions, "Workflow returned exceptions.");
                        throw obj.Exceptions;
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