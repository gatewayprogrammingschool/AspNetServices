using System.Threading.Tasks;

using Newtonsoft.Json;

namespace MDS.MarkdownParser.TestHarness.Core.Helpers;

public static class Json
{
    public static async Task<T> ToObjectAsync<T>(string value)
        => await Task.Run<T>(() =>
        {
            return JsonConvert.DeserializeObject<T>(value);
        });

    public static async Task<string> StringifyAsync(object value)
        => await Task.Run<string>(() =>
        {
            return JsonConvert.SerializeObject(value);
        });
}
