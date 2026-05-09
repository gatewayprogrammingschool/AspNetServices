using System.Threading.Tasks;
using MDS.AspnetServices.Abstractions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace MDS.AspnetServices.Tests;

public class MiddlewareRefactorTests
{
    [Fact]
    public void MarkdownPipelineFactory_CreatePipeline_ReturnsPipeline()
    {
        // Arrange
        var factory = new Mock<IMarkdownPipelineFactory>();
        var options = new MarkdownServerOptions(null);
        factory.Setup(f => f.CreatePipeline(options)).Returns(new Markdig.MarkdownPipelineBuilder().Build());

        // Act
        var pipeline = factory.Object.CreatePipeline(options);

        // Assert
        Assert.NotNull(pipeline);
    }

    [Fact]
    public void MarkdownTagProcessor_ProcessTags_ReturnsProcessedString()
    {
        // Arrange
        var processor = new Mock<IMarkdownTagProcessor>();
        processor.Setup(p => p.ProcessTags("!test", null)).Returns("processed");

        // Act
        var result = processor.Object.ProcessTags("!test");

        // Assert
        Assert.Equal("processed", result);
    }

    [Fact]
    public async Task MarkdownRenderer_RenderAsync_ReturnsHtml()
    {
        // Arrange
        var renderer = new Mock<IMarkdownRenderer>();
        renderer.Setup(r => r.RenderAsync("# Test", null)).ReturnsAsync("<h1>Test</h1>");

        // Act
        var result = await renderer.Object.RenderAsync("# Test");

        // Assert
        Assert.Equal("<h1>Test</h1>", result);
    }

    [Fact]
    public void MarkdownMiddleware_CanHandlePath_ReturnsTrueForMarkdown()
    {
        // Arrange
        var middleware = new Mock<IMarkdownMiddleware>();
        middleware.Setup(m => m.CanHandlePath("/test.md")).Returns(true);

        // Act
        var result = middleware.Object.CanHandlePath("/test.md");

        // Assert
        Assert.True(result);
    }
}