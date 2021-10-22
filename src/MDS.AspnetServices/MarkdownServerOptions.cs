namespace MDS.AspnetServices;

public class MarkdownServerOptions : IOptions<MarkdownServerConfiguration>
{
    public MarkdownServerOptions() {}

    public MarkdownServerOptions(MarkdownServerConfiguration? config)
    {
        if (config is not null)
        {
            Value = config;
        }
    }

    public MarkdownServerOptions(string json)
    {
        if (json is null or "")
        {
            throw new AbandonedMutexException(nameof(json));
        }

        var config = json.FromJson<MarkdownServerConfiguration>();

        if (config is not null)
        {
            Value = config;
        }
    }
    public MarkdownServerConfiguration Value { get; } = new();

    public static MarkdownServerOptions Current { get; internal set; } = new();
}