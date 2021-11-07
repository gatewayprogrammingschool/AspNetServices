using MDS.AppFramework.Controls;
using MDS.AspnetServices.Common;

namespace MDS.TestSite.MdApp.ViewModels
{
    public record IndexViewModel : ControlViewModel
    {
        public string PageTitle => "Page Title in ViewModel.";

        public List<string>? Names { get; set;}

        internal void LoadNames()
        {
            Names = new List<string>()
            {
                "Bob", "Joe", "Billy", "Jim"
            };
        }

        internal IEnumerable<string> SortNames()
        {
            yield return Names?[2] ?? "";
            yield return Names?[1] ?? "";
            yield return Names?[3] ?? "";
            yield return Names?[0] ?? "";
        }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}