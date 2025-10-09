using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.Entities;
using CVAgentApp.Core.Enums;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CVAgentApp.Infrastructure.Services;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly ILogger<DocumentProcessingService> _logger;

    public DocumentProcessingService(ILogger<DocumentProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextFromPDFAsync(Stream pdfStream)
    {
        _logger.LogInformation("Extracting text from PDF");
        
        // TODO: Implement actual PDF text extraction using a library like iTextSharp or PdfPig
        // For now, return placeholder text
        return "PDF content extraction not yet implemented";
    }

    public async Task<string> ExtractTextFromWordAsync(Stream wordStream)
    {
        _logger.LogInformation("Extracting text from Word document");
        
        // TODO: Implement actual Word document text extraction using a library like DocumentFormat.OpenXml
        // For now, return placeholder text
        return "Word document content extraction not yet implemented";
    }

    public async Task<byte[]> GeneratePDFAsync(string content)
    {
        _logger.LogInformation("Generating PDF with QuestPDF");
        await Task.Delay(10); // Minimal delay

        try
        {
            // Set QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;
            
            // Generate PDF using QuestPDF
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text("CV Agent - Generated Document")
                        .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(0.5f, Unit.Centimetre)
                        .Text(content);

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();

            _logger.LogInformation("PDF generated successfully with QuestPDF - {Size} bytes", pdfBytes.Length);
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QuestPDF failed: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            throw; // Don't fall back - let the error be visible
        }
    }

    public async Task<byte[]> GenerateWordAsync(string content)
    {
        _logger.LogInformation("Generating Word document");
        
        try
        {
            // Create a simple Word document structure
            var wordContent = CreateWordDocument(content);
            return System.Text.Encoding.UTF8.GetBytes(wordContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Word document");
            throw;
        }
    }

    private string CreateWordDocument(string content)
    {
        // Create a basic Word document structure
        // This is a simplified approach - in production, use DocumentFormat.OpenXml
        var wordDoc = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    <w:p>
      <w:r>
        <w:t>{content.Replace("\n", "</w:t></w:r></w:p><w:p><w:r><w:t>")}</w:t>
      </w:r>
    </w:p>
  </w:body>
</w:document>";
        
        return wordDoc;
    }

    public async Task<string> FormatDocumentAsync(string content, DocumentType type)
    {
        _logger.LogInformation("Formatting document for type: {DocumentType}", type);
        
        var formattedContent = type switch
        {
            DocumentType.CV => FormatCV(content),
            DocumentType.CoverLetter => FormatCoverLetter(content),
            DocumentType.Portfolio => FormatPortfolio(content),
            _ => content
        };

        _logger.LogInformation("Document formatting completed");
        return formattedContent;
    }

    private string FormatCV(string content)
    {
        // Return content as-is for PDF generation
        return content;
    }

    private string FormatCoverLetter(string content)
    {
        // Return content as-is for PDF generation
        return content;
    }

    private string FormatPortfolio(string content)
    {
        // Return content as-is for PDF generation
        return content;
    }
}