using MDS.AspnetServices.Abstractions;

namespace MDS.AspnetServices.Pdf;

public class PdfOptions
{
    public bool UseCurrentTheme { get; set; } = true;
    public string? ThemeOverride { get; set; }
}

public class PdfManager
{
    private readonly PdfOptions _options;
    private readonly ILogger<PdfManager> _logger;
    private readonly IMarkdownRenderer _renderer;

    public PdfManager(PdfOptions options, ILogger<PdfManager> logger, IMarkdownRenderer renderer)
    {
        _options = options;
        _logger = logger;
        _renderer = renderer;
    }

    public async Task<byte[]> GeneratePdfAsync(string markdown, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF from Markdown");
        var html = await _renderer.RenderAsync(markdown);
        throw new NotImplementedException("TDD Stub - PDF generation");
    }

    public async Task<byte[]> GeneratePdfForPathAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF for path '{Path}'", path);
        throw new NotImplementedException("TDD Stub - PDF for path");
    }
}