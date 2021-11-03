using MDS.AppFramework.Controls;

namespace MDS.TestSite.Controllers
{
    internal record IndexViewModel : ControlViewModel
    {
        public IndexViewModel()
        {
        }

        public IndexViewModel(ControlViewModel original) : base(original)
        {
        }
    }
}