using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MDS.AspnetServices.Common;

public static class JsonUtility
{
    private static JsonSerializerOptions _defaultSettings =
        new(JsonSerializerDefaults.General)
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = false,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            
        };

    public static JsonSerializerOptions CurrentSettings { get; set; } = _defaultSettings;

    public static JsonSerializerOptions DefaultSettings => _defaultSettings;

    public static string ToJson(this object? source, JsonSerializerOptions? settings = default)
    {
        var toSerialize = source ?? new();

        var json = JsonSerializer.Serialize(toSerialize, settings ?? CurrentSettings);

        return json;
    }

    public static Task SerializeAsync(this Stream stream,
                                      object toSerialize,
                                      JsonSerializerOptions? settings = default,
                                      CancellationToken token = default)
        => JsonSerializer.SerializeAsync(stream,
                                         toSerialize,
                                         settings ?? CurrentSettings,
                                         token);

    public static TObject? FromJson<TObject>(
        this string? json,
        JsonSerializerOptions? settings = default)
        where TObject : new()
        => json is null or ""
            ? new()
            : JsonSerializer.Deserialize<TObject>(json, settings ?? CurrentSettings);

    public static object? FromJson(
        this string? json,
        JsonSerializerOptions? settings = default)
        => json is null or ""
            ? new()
            : JsonSerializer.Deserialize(json, typeof(object), settings ?? CurrentSettings);

    public static ValueTask<TData?> DeserializeAsync<TData>(
        this Stream stream,
        JsonSerializerOptions? settings = default,
        CancellationToken token = default)
        => JsonSerializer.DeserializeAsync<TData>(stream, settings ?? CurrentSettings, token);

    public static object? FromJson(
        this string? json,
        string typeName,
        JsonSerializerOptions? settings = default)
    {
        if (json is null or "")
        {
            return new();
        }

        var type = Type.GetType(typeName);

        type ??= typeof(object);

        return JsonSerializer.Deserialize(json, type, settings ?? CurrentSettings);
    }

    public static TObject WithJson<TObject>(
        this TObject? instance,
        string? json,
        JsonSerializerOptions? settings = default)
            where TObject : new()
    {
        instance ??= new();

        if (json is null or "")
        {
            return instance;
        }

        var deserialized = JsonSerializer.Deserialize(json, instance.GetType(), settings ?? CurrentSettings);

        deserialized?.PopulateInto(instance, BindingFlags.Instance | BindingFlags.Public);

        return instance;
    }

    /// <summary>
    /// Retrieves a property from the object.
    /// </summary>
    /// <param name="source">The object containing the property.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="flags">The BindingFlags to use to
    /// get the property.</param>
    /// <returns>An instance of PropertyInfo for the
    /// specified property.</returns>
    private static PropertyInfo? GetProperty(
        this object source,
        string name,
        BindingFlags flags)
    {
        var properties = source.GetType().GetProperties(flags)
                        ?? Array.Empty<PropertyInfo>();

        return Array.Find(properties, property => property.Name == name);
    }

    /// <summary>
    /// Populates common properties of the target from the source.
    /// </summary>
    /// <param name="source">The object containing the source values.</param>
    /// <param name="target">The object to populate.</param>
    /// <param name="flags">BindingFlags of the operation.</param>
    private static void PopulateInto(
        this object source,
        object target,
        BindingFlags flags)
    {
        foreach (var pi in source.GetType().GetProperties(flags))
        {
            var tpi = target.GetProperty(pi.Name, flags);

            if (tpi is not null)
            {
                try
                {
                    object? originalValue;
                    object? value = originalValue = source.GetValue<object>(pi.Name, flags);

                    if (originalValue is null && value is null)
                    {
                        continue;
                    }

                    try
                    {
                        value = Convert.ChangeType(originalValue, tpi.PropertyType);
                    }
                    catch
                    {
                    }

                    tpi.SetValue(target, value, Array.Empty<object>());
                }
                catch
                {
                }
            }
        }
    }

    /// <summary>
    /// Gets the value of the specified property
    /// using reflection.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="source">The object containing the value to return.</param>
    /// <param name="name">The name of the property to return.</param>
    /// <param name="flags">The binding flags to use.</param>
    /// <returns>The typed value.</returns>
    private static T? GetValue<T>(
        this object source,
        string name,
        BindingFlags flags)
    {
        PropertyInfo? pi = source.GetProperty(name, flags);

        return (T?)(pi?.GetValue(source));
    }

}