using MDS.AspnetServices.Abstractions;

namespace MDS.AspnetServices.Stubs;

public class MarkdownRendererStub : IMarkdownRenderer
{
    public Task<string> RenderAsync(string markdown, object? context = null)
    {
        throw new NotImplementedException("TDD Stub - MarkdownRendererStub");
    }
}