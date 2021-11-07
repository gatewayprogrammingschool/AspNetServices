using MDS.AppFramework.Controls;
using MDS.TestSite.MdApp.ViewModels;

namespace MDS.TestSite.MdApp.Views
{
    public partial record IndexView(string Id) : ViewWorkflow(Id), IViewWorkflow
    {
        public IndexView() : this(Guid.NewGuid().ToString())
        {
        }

        public override ControlViewModel? ViewModel {get;set;} = new IndexViewModel();

        public override Task BuildControlsAsync(HttpContext context)
        {
            // Be sure to give a logger to each control!
            return Task.CompletedTask;
        }

        public override Task InitAsync(HttpContext context)
        {
            return base.InitAsync(context);
        }
    }
}