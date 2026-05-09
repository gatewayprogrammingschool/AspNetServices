using System;
using System.Collections.Concurrent;
using System.IO;
using Xunit;

namespace MDS.AspnetServices.Tests;

public class GlobalYamlDefaultsTests
{
    [Fact]
    public void LoadDefaults_ReturnsEmpty_WhenFileIsNull()
    {
        var result = GlobalYamlDefaults.LoadDefaults("");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void LoadDefaults_ReturnsEmpty_WhenNoGlobalYamlExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var testFile = Path.Combine(tempDir, "test.md");
            File.WriteAllText(testFile, "# Hello");
            var result = GlobalYamlDefaults.LoadDefaults(testFile);
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadDefaults_FindsGlobalYaml_InSameDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var globalYaml = Path.Combine(tempDir, "global.yaml");
            File.WriteAllText(globalYaml, "title: My Title\nlayout: default");

            var testFile = Path.Combine(tempDir, "test.md");
            File.WriteAllText(testFile, "# Hello");

            var result = GlobalYamlDefaults.LoadDefaults(testFile);
            Assert.Equal(2, result.Count);
            Assert.Equal("My Title", result["title"]);
            Assert.Equal("default", result["layout"]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadDefaults_FindsParentGlobalYaml_WhenChildDoesNotHaveOne()
    {
        var parentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var childDir = Path.Combine(parentDir, "subdir");
        Directory.CreateDirectory(childDir);
        try
        {
            var globalYaml = Path.Combine(parentDir, "global.yaml");
            File.WriteAllText(globalYaml, "title: Parent Title\nlayout: parent");

            var testFile = Path.Combine(childDir, "test.md");
            File.WriteAllText(testFile, "# Hello");

            var result = GlobalYamlDefaults.LoadDefaults(testFile);
            Assert.Equal("Parent Title", result["title"]);
            Assert.Equal("parent", result["layout"]);
        }
        finally
        {
            Directory.Delete(parentDir, true);
        }
    }

    [Fact]
    public void LoadDefaults_ChildOverridesParent()
    {
        var parentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var childDir = Path.Combine(parentDir, "subdir");
        Directory.CreateDirectory(childDir);
        try
        {
            var parentYaml = Path.Combine(parentDir, "global.yaml");
            File.WriteAllText(parentYaml, "title: Parent Title\nlayout: parent");

            var childYaml = Path.Combine(childDir, "global.yaml");
            File.WriteAllText(childYaml, "title: Child Title");

            var testFile = Path.Combine(childDir, "test.md");
            File.WriteAllText(testFile, "# Hello");

            var result = GlobalYamlDefaults.LoadDefaults(testFile);
            Assert.Equal("Child Title", result["title"]);
            Assert.Equal("parent", result["layout"]);
        }
        finally
        {
            Directory.Delete(parentDir, true);
        }
    }

    [Fact]
    public void LoadDefaults_HandlesNestedKeys()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var globalYaml = Path.Combine(tempDir, "global.yaml");
            File.WriteAllText(globalYaml, "Variables:\n  title: My Title\n  author: Test Author");

            var testFile = Path.Combine(tempDir, "test.md");
            File.WriteAllText(testFile, "# Hello");

            var result = GlobalYamlDefaults.LoadDefaults(testFile);
            Assert.Equal("My Title", result["Variables.title"]);
            Assert.Equal("Test Author", result["Variables.author"]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadDefaults_IgnoresMalformedYaml()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var globalYaml = Path.Combine(tempDir, "global.yaml");
            File.WriteAllText(globalYaml, "{{ invalid yaml }");

            var testFile = Path.Combine(tempDir, "test.md");
            File.WriteAllText(testFile, "# Hello");

            var result = GlobalYamlDefaults.LoadDefaults(testFile);
            Assert.Empty(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}