namespace MDS.TestSite.MdApp.Views
{
    using AppFramework.Controls;

    using ViewModels;

    public partial record IndexView(string Id) : ViewWorkflow(Id), IViewWorkflow
    {
        public new IndexViewModel? ViewModel
        {
            get => base.ViewModel as IndexViewModel;
            set => base.ViewModel = value;
        }

        public override Task InitAsync(HttpContext context)
        {
            ViewModel = new();

            return base.InitAsync(context);
        }

        public IndexView()
            : this(
                Guid.NewGuid()
                    .ToString()
            )
        {
        }

        public override Task BuildControlsAsync(HttpContext context)
            // Be sure to give a logger to each control!
            => Task.CompletedTask;
    }
}
