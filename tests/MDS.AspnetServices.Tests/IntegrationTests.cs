using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MDS.AspnetServices.Abstractions;
using MDS.AspnetServices.Stubs;

namespace MDS.AspnetServices.Tests;

public class IntegrationTests
{
    [Fact]
    public void CanRegisterStubsInDI()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMarkdownPipelineFactory, MarkdownPipelineFactoryStub>();
        services.AddScoped<IMarkdownTagProcessor, MarkdownTagProcessorStub>();
        services.AddScoped<IMarkdownRenderer, MarkdownRendererStub>();
        services.AddScoped<IMarkdownMiddleware, MarkdownMiddlewareStub>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IMarkdownPipelineFactory>();
        var processor = scope.ServiceProvider.GetRequiredService<IMarkdownTagProcessor>();
        var renderer = scope.ServiceProvider.GetRequiredService<IMarkdownRenderer>();
        var middleware = scope.ServiceProvider.GetRequiredService<IMarkdownMiddleware>();

        Assert.NotNull(factory);
        Assert.NotNull(processor);
        Assert.NotNull(renderer);
        Assert.NotNull(middleware);
    }
}
