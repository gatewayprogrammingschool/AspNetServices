#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
using System.Collections.Concurrent;
using System.Reflection;

using MDS.AppFramework.Controls;
using MDS.AspnetServices;
using MDS.AspnetServices.Common;

using Microsoft.AspNetCore.WebUtilities;

using Newtonsoft.Json;

namespace MDS.AppFramework.Common
{
    internal class MarkdownApplicationMiddleware
    {
        public MarkdownApplicationOptions Options
        {
            get;
        }

        public MarkdownApplicationConfiguration Value
        {
            get;
        } = new();

        public static MarkdownApplicationOptions? Current
        {
            get;
            private set;
        }

        public string? ServerRoot
        {
            get;
            set;
        }

        public IServiceProvider? Services
        {
            get;
            set;
        }

        public MarkdownApplicationMiddleware(
            RequestDelegate next,
            MarkdownApplicationOptions options
        )
        {
            _next = next;
            Options = options;
            _logger = Options.Services.GetRequiredService<ILogger<MarkdownApplicationMiddleware>>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using ManualResetEventSlim mre = new(false);
            PathString path = context.Request.Path;

            try
            {
                if (!(path.Value?.StartsWith(
                          "/mdapp/",
                          StringComparison.InvariantCultureIgnoreCase
                      ) ??
                      false))
                {
                    await _next.Invoke(context);

                    return;
                }

                var map = Options.Services.GetRequiredService<ControllerMap>();

                PathControllerMapItem controllerMapItem = map.GetControllerType(path);

                if (controllerMapItem == default)
                {
                    _logger.LogWarning($"No controller found for {path}");
                    await _next.Invoke(context);

                    return;
                }

                AppController controller = AppController.BeginContext(
                    controllerMapItem,
                    context,
                    Options.Services
                )!;

                if (controllerMapItem.Method is
                    {
                        Length: > 0,
                    })
                {
                    MethodInfo? methodInfo
                        = controllerMapItem.ControllerType!.GetMethod(controllerMapItem.Method);

                    if (methodInfo is null)
                    {
                        await InvokeViewMonitor(
                            context,
                            mre,
                            path,
                            controller
                        );
                    }
                    else
                    {
                        try
                        {
                            IResult? result = null;

                            if (methodInfo.ReturnType.IsAssignableTo(typeof(IResult)))
                            {
                                var p = methodInfo.GetParameters();

                                if (p is
                                    {
                                        Length: 1,
                                    } &&
                                    context.Request.ContentType is
                                        "application/x-www-form-urlencoded" &&
                                    p[0]
                                        .ParameterType.IsAssignableTo(typeof(ControlViewModel)))
                                {
                                    object formCollection = Activator.CreateInstance(
                                        p[0]
                                            .ParameterType,
                                        new object[]
                                        {
                                            await context.Request.ReadFormAsync(),
                                        }
                                    );
                                    result = (IResult)methodInfo.Invoke(
                                        controller,
                                        new object?[]
                                        {
                                            formCollection,
                                        }
                                    );
                                }
                                else if (context.Request.ContentType is not
                                         "application/x-www-form-urlencoded")
                                {
                                    object formCollection = Activator.CreateInstance(
                                        p[0]
                                            .ParameterType,
                                        new object?[]
                                        {
                                            JsonConvert.DeserializeObject(
                                                context.Request.BodyReader.ToJson()
                                            ),
                                        }
                                    );
                                    result = (IResult)methodInfo.Invoke(
                                        controller,
                                        new object?[]
                                        {
                                            formCollection,
                                        }
                                    );
                                }
                                else
                                {
                                    result = (IResult)methodInfo.Invoke(
                                        controller,
                                        Array.Empty<object>()
                                    );
                                }

                                if (result is not null)
                                {
                                    await result.ExecuteAsync(context);
                                }
                                else
                                {
                                    throw new ApplicationException(
                                        $"No result returned by {methodInfo.Name}."
                                    );
                                }

                                mre.Set();
                            }
                            else
                            {
                                throw new ApplicationException(
                                    $"{methodInfo.Name} does not return IResult."
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException(
                                $"Exception calling {methodInfo.Name}.",
                                ex
                            );
                        }
                    }
                }
                else
                {
                    await InvokeViewMonitor(
                        context,
                        mre,
                        path,
                        controller
                    );
                }

                if (mre.Wait(TimeSpan.FromMinutes(5)))
                {
                    _logger.LogInformation(
                        $"Session {context.Session.Id} call to {path} completed successfully in {GetType().Name}."
                    );

                    return;
                }

                _logger.LogWarning(
                    $"Session {context.Session.Id} call to {path} timed out in {GetType().Name}."
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{GetType().Name} handled exception.");
                await new MarkdownResponse(e).ToMarkdownResult()
                    .ExecuteAsync(context);
            }

            Task ViewMonitor_ViewNotCompleted(
                IViewWorkflow obj,
                AggregateException ae,
                CancellationToken token
            )
            {
                if (ae is not null
                    and
                    {
                        InnerExceptions.Count: > 0,
                    })
                {
                    _logger.LogError(ae, "Workflow returned exceptions.");

                    throw ae;
                }

                _logger.LogWarning(
                    $"Session {context.Session.Id} call to {path} did not complete."
                );

                mre.Set();

                return Task.CompletedTask;
            }

            async Task ViewMonitor_ViewCompleted(IViewWorkflow workflow, CancellationToken token)
            {
                if (workflow.Exceptions?.InnerExceptions.Any() ?? false)
                {
                    _logger.LogError(workflow.Exceptions, "Workflow returned exceptions.");

                    throw workflow.Exceptions;
                }

                HttpResponseStreamWriter? stream = workflow.Renderer;

                if (stream is null)
                {
                    throw new ApplicationException("Workflow provides no RenderStream.");
                }

                //await stream.FlushAsync().ConfigureAwait(false);

                var response = workflow.StringBuilder.ToString();

                // Get the Markdown file for the view
                string viewName = workflow.GetType()
                    .FullName!;
                int index = viewName.IndexOf(".mdapp.", StringComparison.OrdinalIgnoreCase);

                if (index > -1)
                {
                    index++;
                    var mdappName = viewName[index..]
                                        .Replace(".", "\\") +
                                    ".md";

                    if (File.Exists(mdappName))
                    {
                        var filename = new FileInfo(mdappName).FullName;
                        ConcurrentDictionary<string, object> variables = new();
                        _ = variables.TryAdd(nameof(workflow.ViewKey), workflow.ViewKey);
                        _ = variables.TryAdd($"ViewBody:{workflow.ViewKey}", response);
                        _ = variables.TryAdd(nameof(viewName), viewName);

                        if (workflow.ViewModel is not null)
                        {
                            await ProcessViewModelAsync(workflow, variables);
                        }

                        var options = Options.Services.GetRequiredService<MarkdownServerOptions>();
                        var result
                            = await options.MarkdownFileExecute(context, filename, variables);
                        await result.ExecuteAsync(context);
                    }
                }

                _logger.LogInformation($"Session {context.Session.Id} call to {path} completed.");
                mre.Set();
            }

            async Task InvokeViewMonitor(
                HttpContext context,
                ManualResetEventSlim mre,
                PathString path,
                AppController controller
            )
            {
                var viewMonitor = controller.GetViewMonitor(path);

                viewMonitor.ViewCompletedAsync -= ViewMonitor_ViewCompleted;
                viewMonitor.ViewCompletedAsync += ViewMonitor_ViewCompleted;

                viewMonitor.ViewNotCompletedAsync -= ViewMonitor_ViewNotCompleted;
                viewMonitor.ViewNotCompletedAsync += ViewMonitor_ViewNotCompleted;

                await viewMonitor.StartAsync(context, controller.CancellationTokenSource.Token);
            }
        }

        private readonly ILogger<MarkdownApplicationMiddleware> _logger;
        private readonly RequestDelegate _next;

        private async Task ProcessViewModelAsync(
            IViewState viewState,
            ConcurrentDictionary<string, object> variables
        )
        {
            LazyContainer? lazy = viewState.ViewState.GetValueOrDefault(nameof(IAppView.ViewModel));
            ControlViewModel? viewModel = lazy is not null
                ? await lazy.GetLazyDataAsync<ControlViewModel>()
                : null;

            if (viewModel is not null)
            {
                foreach (PropertyInfo pi in viewModel?.GetType()
                                                .GetProperties(
                                                    BindingFlags.Instance | BindingFlags.Public
                                                ) ??
                                            Array.Empty<PropertyInfo>())
                {
                    if (pi.GetMethod?.GetParameters()
                            ?.Any() ??
                        false)
                    {
                        continue;
                    }

                    try
                    {
                        var name = $"ViewModel.{pi.Name}";
                        var value = pi.GetValue(viewModel) as string ??
                                    pi.GetValue(viewModel)
                                        ?.ToString() ??
                                    string.Empty;

                        _ = variables.AddOrUpdate(name, value, (_, _) => value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Exception attempting to populate [{pi.Name}]");

                        throw;
                    }
                }

                _ = variables.AddOrUpdate(
                    "ViewModel",
                    viewModel?.ToString() ?? "",
                    (_, _) => viewModel?.ToString() ?? ""
                );
            }
        }
    }
}
