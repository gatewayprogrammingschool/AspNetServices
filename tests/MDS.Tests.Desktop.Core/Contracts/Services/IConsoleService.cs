using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

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
