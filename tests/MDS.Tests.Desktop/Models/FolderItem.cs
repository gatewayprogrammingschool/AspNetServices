namespace MDS.Tests.Desktop.Models;

public record struct FolderItem(string RelativePath, string FullPath)
{
    public static implicit operator (string RelativePath, string FullPath)(FolderItem value)
    {
        return (value.RelativePath, value.FullPath);
    }

    public static implicit operator FolderItem((string RelativePath, string FullPath) value)
    {
        return new FolderItem(value.RelativePath, value.FullPath);
    }
}