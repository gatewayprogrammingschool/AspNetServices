namespace MDS.AspnetServices;

public class MarkdownServerConfiguration
{
    public string? LayoutFile { get; set; } = "./wwwroot/DefaultLayout.html";
    public string? DefaultPath
    {
        get; set;
    } = "index.md";
}