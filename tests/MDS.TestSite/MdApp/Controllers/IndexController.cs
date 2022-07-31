using System;

namespace MDS.TestSite.MdApp.Controllers;

using System.Text;

using AppFramework;

using AspnetServices.Common;

using ViewModels;

using Views;

public record IndexController(IServiceProvider Services, string Id) : AppController(Services)
{
    public override TViewMonitor GetViewMonitor<TViewMonitor>(PathString path)
    {
        var monitor = Services.GetRequiredService<IndexViewMonitor>();
        monitor.Path = path;

        return (TViewMonitor)(monitor as IViewMonitor);
    }

    public IResult Search(IndexViewModel viewModel)
    {
        StringBuilder sb = new();

        sb.AppendLine("# Result");
        sb.AppendLine();
        sb.AppendLine($"## Seach: {viewModel.q}");
        sb.AppendLine();

        return new MarkdownResult(sb.ToString());
    }
}
