namespace MDS.AspnetServices;

public class MarkdownServerOptions : IOptions<MarkdownServerConfiguration>
{
    public MarkdownServerOptions(IServiceProvider services)
    {
        Services = services;
        Current = this;
    }

    public MarkdownServerOptions(
        IServiceProvider services, 
        MarkdownServerConfiguration? config) : this(services)
    {
        if (config is not null)
        {
            Value = config;
        }
    }

    public MarkdownServerOptions(
        IServiceProvider services, 
        string json) : this(services)
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

    public static MarkdownServerOptions? Current { get; private set; }
    private string? _serverRoot;

    public string? ServerRoot
    {
        get => _serverRoot;
        set => _serverRoot = value;
    }

    public IServiceProvider Services { get; set; }
}