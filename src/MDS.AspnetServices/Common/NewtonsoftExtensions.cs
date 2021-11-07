// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

using System.ComponentModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NewtonsoftSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;

using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace MDS.AspnetServices.Common;

internal static class NewtonsoftExtensions
{
    private static readonly NewtonsoftSerializerSettings _defaultSettings = new()
    {
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Error,
        NullValueHandling = NullValueHandling.Ignore,
        Error = Error,
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    private static void Error(object? sender, ErrorEventArgs e)
    {
        e.ErrorContext.Handled = true;
    }

    public static NewtonsoftSerializerSettings CurrentSettings { get; set; } = _defaultSettings;

    public static NewtonsoftSerializerSettings DefaultSettings => _defaultSettings;

    public static string ToNsJson(this object? source, NewtonsoftSerializerSettings? settings = default)
    {
        var toSerialize = source ?? new();

        var json = JsonConvert.SerializeObject(toSerialize, settings ?? CurrentSettings);

        return json;
    }

    public static TObject? FromNsJson<TObject>(
        this string? json,
        NewtonsoftSerializerSettings? settings = default)
        where TObject : new()
        => json is null or ""
            ? new()
            : JsonConvert.DeserializeObject<TObject>(json, settings ?? CurrentSettings);

    public static object? FromNsJson(
        this string? json,
        NewtonsoftSerializerSettings? settings = default)
        => json is null or ""
            ? new()
            : JsonConvert.DeserializeObject(json, settings ?? CurrentSettings);

    public static object? FromNsJson(
        this string? json,
        string typeName,
        NewtonsoftSerializerSettings? settings = default)
    {
        if (json is null or "")
        {
            return new();
        }

        var type = Type.GetType(typeName);

        type ??= typeof(object);

        return JsonConvert.DeserializeObject(json, type, settings ?? CurrentSettings);
    }

    public static TObject WithNsJson<TObject>(
        this TObject? instance,
        string? json,
        NewtonsoftSerializerSettings? settings = default)
            where TObject : new()
    {
        instance ??= new();

        if (json is null or "")
        {
            return instance;
        }

        JsonConvert.PopulateObject(json, instance, settings ?? CurrentSettings);

        return instance;
    }
}
