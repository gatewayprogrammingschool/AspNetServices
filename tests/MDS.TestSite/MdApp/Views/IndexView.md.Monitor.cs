#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using MDS.AppFramework;
using MDS.AppFramework.Controls;

namespace MDS.TestSite.MdApp.Views
{
    internal record IndexViewMonitor(IViewWorkflow View, ILogger Logger) :
        ViewMonitor(View, Logger)
    {
        public IndexViewMonitor(ILogger<IndexViewMonitor> logger) :
            this(new IndexView(typeof(IndexView).Name), logger)
        {

        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
