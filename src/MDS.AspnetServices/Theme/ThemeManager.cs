using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Common;
using MDS.AspnetServices.Abstractions;

namespace MDS.AspnetServices.Theme;

public class ThemeOptions
{
    public string ThemesDirectory { get; set; } = "themes";
    public List<string> PackageSources { get; set; } = new() { "https://api.nuget.org/v3/index.json" };
}

public class ThemeManager
{
    private readonly ThemeOptions _options;
    private readonly ILogger<ThemeManager> _logger;

    public ThemeManager(ThemeOptions options, ILogger<ThemeManager> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<List<ThemeInfo>> SearchThemesAsync(string query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching themes for '{Query}'", query);
        throw new NotImplementedException("TDD Stub - Theme search");
    }

    public async Task InstallThemeAsync(string packageId, string version, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Installing theme '{PackageId}@{Version}'", packageId, version);
        throw new NotImplementedException("TDD Stub - Theme install");
    }
}

public record ThemeInfo
{
    public string Id { get; init; } = "";
    public string Version { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string? Authors { get; init; }
    public string? IconUrl { get; init; }
}