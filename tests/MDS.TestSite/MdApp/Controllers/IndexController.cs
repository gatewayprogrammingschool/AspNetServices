using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MDS.AppFramework;
using MDS.TestSite.MdApp.Views;

namespace MDS.TestSite.MdApp.Controllers
{
    public record IndexController(IServiceProvider Services, string Id) : AppController(Services)
    {
        public override TViewMonitor GetViewMonitor<TViewMonitor>(PathString path)
        {
            var monitor = Services.GetRequiredService<IndexViewMonitor>();
            monitor.Path = path;

            return (TViewMonitor)(monitor as IViewMonitor);
        }
    }
}
