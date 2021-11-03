using MDS.AppFramework.Controls;

namespace MDS.AppFramework;

public interface IAppView
{
    string ViewKey { get; }

    ControlViewModel? ViewModel {get; set; }
}