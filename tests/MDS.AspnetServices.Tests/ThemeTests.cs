using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MDS.AspnetServices.Theme;

namespace MDS.AspnetServices.Tests;

public class ThemeTests
{
    [Fact]
    public async Task SearchThemesAsync_ReturnsThemes()
    {
        // Arrange
        var options = new ThemeOptions();
        var logger = Mock.Of<ILogger<ThemeManager>>();
        var manager = new ThemeManager(options, logger);

        // Mock NuGet search (stub)
        // Full mock would require IPackageSearchMetadataService mock

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => manager.SearchThemesAsync("test"));
    }

    [Fact]
    public async Task InstallThemeAsync_ThrowsNotImplemented()
    {
        // Arrange
        var options = new ThemeOptions();
        var logger = Mock.Of<ILogger<ThemeManager>>();
        var manager = new ThemeManager(options, logger);

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => manager.InstallThemeAsync("test", "1.0"));
    }
}