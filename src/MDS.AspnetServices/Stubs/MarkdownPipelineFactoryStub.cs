using Markdig;
using MDS.AspnetServices.Abstractions;

namespace MDS.AspnetServices.Stubs;

public class MarkdownPipelineFactoryStub : IMarkdownPipelineFactory
{
    public MarkdownPipeline CreatePipeline(MarkdownServerOptions options)
    {
        throw new NotImplementedException("TDD Stub - MarkdownPipelineFactoryStub");
    }

    public MarkdownPipeline GetDefaultPipeline()
    {
        throw new NotImplementedException("TDD Stub - MarkdownPipelineFactoryStub");
    }

    public MarkdownPipeline GetPipelineForTheme(string themeName)
    {
        throw new NotImplementedException("TDD Stub - MarkdownPipelineFactoryStub");
    }
}