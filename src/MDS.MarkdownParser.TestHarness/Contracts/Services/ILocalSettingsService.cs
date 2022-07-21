using System.Threading.Tasks;

namespace MDS.MarkdownParser.TestHarness.Contracts.Services;

public interface ILocalSettingsService
{
    Task<T> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
}
