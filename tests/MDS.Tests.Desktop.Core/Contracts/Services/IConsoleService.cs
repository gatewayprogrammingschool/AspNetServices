using System.ComponentModel;

namespace MDS.Tests.Desktop.Core.Contracts.Services;

public interface IConsoleService
{
    string STDERR
    {
        get;
    }
    string STDOUT
    {
        get;
    }

    event PropertyChangedEventHandler PropertyChanged;

    string ResetStdErr();
    string ResetStdOut();
}
