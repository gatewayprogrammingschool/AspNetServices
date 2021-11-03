#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using MDS.AppFramework;
using MDS.AppFramework.Controls;

namespace MDS.TestSite.Controllers
{
    internal record IndexViewMonitor(IViewWorkflow View, ILogger Logger) :
        ViewMonitor(View, Logger)
    {
        public IndexViewMonitor(ILogger<IndexViewMonitor> logger) : 
            this(new IndexViewWorkflow(Guid.NewGuid().ToString()), logger)
        {
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
