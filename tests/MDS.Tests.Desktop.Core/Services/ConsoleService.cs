using System.ComponentModel;
using System.Text;
using System.Timers;
using MDS.Tests.Desktop.Core.Contracts.Services;

using Timer = System.Timers.Timer;

namespace MDS.Tests.Desktop.Core.Services;

public class ConsoleService : INotifyPropertyChanged, IConsoleService
{
    private readonly Timer _timer = new(TimeSpan.FromSeconds(1));
    private readonly Stream _out;
    private readonly StreamReader _outReader;
    private readonly Stream _err;
    private readonly StreamReader _errReader;

    private readonly StringBuilder _stdout = new();
    private readonly StringBuilder _stderr = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public string STDOUT => _stdout.ToString();
    public string STDERR => _stderr.ToString();

    public ConsoleService()
    {
        _err = Console.OpenStandardOutput();
        _outReader = new StreamReader(_err);
        _err = Console.OpenStandardError();
        _errReader = new StreamReader(_err);

        _timer.Elapsed += _timer_Elapsed;

        _timer.Start();
    }

    public string ResetStdOut()
    {
        _timer.Stop();
        try
        {
            _out.Flush();

            var text = Read(_outReader, nameof(STDOUT));

            _stdout.Clear();

            return text;
        }
        finally
        {
            _timer.Start();
        }
    }

    public string ResetStdErr()
    {
        _timer.Stop();
        try
        {
            _err.Flush();

            var text = Read(_errReader, nameof(STDERR));

            _stderr.Clear();

            return text;
        }
        finally
        {
            _timer.Start();
        }
    }

    private void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        try
        {
            Read(_outReader, nameof(STDOUT));
            Read(_errReader, nameof(STDERR));
        }
        finally
        {
            _timer.Start();
        }
    }

    private string Read(StreamReader reader, string name)
    {
        var outText = reader.ReadToEnd();

        if (outText.Length > 0)
        {
            _stdout.Append(outText);
            PropertyChanged?.Invoke(this, new(name));
        }

        return outText;
    }
}
