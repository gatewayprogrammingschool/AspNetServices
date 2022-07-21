using MDS.AppFramework.Common;
using MDS.AppFramework.Controls;
using MDS.TestSite.MdApp.ViewModels;

namespace MDS.TestSite.MdApp.Views
{
    public partial record IndexView(string Id) : ViewWorkflow(Id), IViewWorkflow
    {
        public IndexView() : this(Guid.NewGuid().ToString())
        {
        }

        public new IndexViewModel? ViewModel
        {
            get => base.ViewModel as IndexViewModel;
            set => base.ViewModel = value;
        }

        public override Task BuildControlsAsync(HttpContext context)
            // Be sure to give a logger to each control!
            => Task.CompletedTask;

        public override Task InitAsync(HttpContext context)
        {
            ViewModel = new();

            return base.InitAsync(context);
        }
    }
}