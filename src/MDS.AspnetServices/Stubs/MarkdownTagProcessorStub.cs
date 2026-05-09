using MDS.AspnetServices.Abstractions;

namespace MDS.AspnetServices.Stubs;

public class MarkdownTagProcessorStub : IMarkdownTagProcessor
{
    public string ProcessTags(string markdown, object? context = null)
    {
        throw new NotImplementedException("TDD Stub - MarkdownTagProcessorStub");
    }

    public bool TryProcessTag(string tag, out string result)
    {
        result = default!;
        throw new NotImplementedException("TDD Stub - MarkdownTagProcessorStub");
    }
}