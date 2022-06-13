using MDS.AppFramework.Controls;
using MDS.AspnetServices.Common;

namespace MDS.TestSite.MdApp.ViewModels
{
    public record IndexViewModel : ControlViewModel
    {
        public IndexViewModel(IFormCollection collection) : base(collection) { }
        public IndexViewModel() : base() { }

        public string PageTitle => "Page Title in ViewModel.";

        public List<string>? Names
        {
            get => (Values.TryGetValue(nameof(Names), out var values) ? values : default).ToList();
            set => Values.AddOrUpdate(nameof(Names), _ => new(value?.ToArray()), (_, _) => new(value?.ToArray()));
        }

        public string? q {
            get => (Values.TryGetValue(nameof(q), out var values) ? values : default).FirstOrDefault();
            set => Values.AddOrUpdate(nameof(q), _ => new(value), (_, _) => new(value));
        }

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