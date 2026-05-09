using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MDS.AspnetServices.Abstractions;

namespace MDS.AspnetServices.PageLifecycle;

public class PageLifecycleRuntime
{
    private readonly IMarkdownRenderer _renderer;

    public PageLifecycleRuntime(IMarkdownRenderer renderer)
    {
        _renderer = renderer;
    }

    public async Task<object> InstantiatePageAsync(IParseTree parseTree)
    {
        throw new NotImplementedException("TDD Stub - Page lifecycle instantiation from parse tree");
    }

    public async Task OnInitializedAsync(object component)
    {
        throw new NotImplementedException("TDD Stub - OnInitializedAsync");
    }

    public async Task OnParametersSetAsync(object component)
    {
        throw new NotImplementedException("TDD Stub - OnParametersSetAsync");
    }
}