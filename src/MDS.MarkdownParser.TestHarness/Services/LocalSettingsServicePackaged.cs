using System.Threading.Tasks;

using MDS.MarkdownParser.TestHarness.Contracts.Services;
using MDS.MarkdownParser.TestHarness.Core.Helpers;

using Windows.Storage;

namespace MDS.MarkdownParser.TestHarness.Services;

public class LocalSettingsServicePackaged : ILocalSettingsService
{
    public async Task<T> ReadSettingAsync<T>(string key)
    {
        if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
        {
            return await Json.ToObjectAsync<T>((string)obj);
        }

        return default;
    }

    public async Task SaveSettingAsync<T>(string key, T value)
        => ApplicationData.Current.LocalSettings.Values[key] = await Json.StringifyAsync(value);
}
