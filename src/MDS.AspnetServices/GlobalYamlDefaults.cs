using YamlDotNet.Serialization;

namespace MDS.AspnetServices;

/// <summary>
/// Loads and merges global.yaml defaults by walking up the directory tree
/// from a markdown file's location toward the root.
/// Closer global.yaml files override farther ones.
/// Per-file front-matter should be applied after this, so per-file wins.
/// </summary>
public static class GlobalYamlDefaults
{
    private static readonly Deserializer Deserializer = new();

    /// <summary>
    /// Walks up from <paramref name="filePath"/>'s directory toward the root,
    /// finds all global.yaml files, merges them (closer wins), and returns
    /// a flattened dictionary of variables.
    /// </summary>
    public static ConcurrentDictionary<string, object> LoadDefaults(string filePath)
    {
        var result = new ConcurrentDictionary<string, object>();

        if (string.IsNullOrEmpty(filePath))
            return result;

        var dir = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (string.IsNullOrEmpty(dir))
            return result;

        // Collect global.yaml files from farthest (root) to closest (file's dir)
        var globalFiles = new List<string>();
        var current = dir;

        while (true)
        {
            var candidate = Path.Combine(current, "global.yaml");
            if (File.Exists(candidate))
                globalFiles.Add(candidate);

            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrEmpty(parent) || parent == current)
                break;
            current = parent;
        }

        // Reverse so farthest is loaded first, closest overrides
        globalFiles.Reverse();

        foreach (var gf in globalFiles)
        {
            try
            {
                var yaml = File.ReadAllText(gf);
                var dict = Deserializer.Deserialize<Dictionary<object, object>>(yaml);
                if (dict == null) continue;

                FlattenAndMerge("", dict, result);
            }
            catch
            {
                // Skip malformed global.yaml files silently
            }
        }

        return result;
    }

    private static void FlattenAndMerge(
        string prefix,
        Dictionary<object, object> source,
        ConcurrentDictionary<string, object> target)
    {
        foreach (var kvp in source)
        {
            var key = kvp.Key?.ToString() ?? "";
            var path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

            if (kvp.Value is Dictionary<object, object> nested)
            {
                FlattenAndMerge(path, nested, target);
            }
            else
            {
                target[path] = kvp.Value ?? "";
            }
        }
    }
}