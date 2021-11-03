using MDS.AppFramework.Controls;

namespace MDS.TestSite.Controllers
{
    public record IndexViewWorkflow(string Id) : ViewWorkflow(Id), IViewWorkflow
    {
        public IndexViewWorkflow () : this(Guid.NewGuid().ToString())
        {

        }

        public override Task BuildControlsAsync(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public override Task InitAsync(HttpContext context)
        {
            return base.InitAsync(context);
        }
    }
}