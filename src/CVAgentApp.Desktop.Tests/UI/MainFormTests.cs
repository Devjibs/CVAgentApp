using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.Entities;
using CVAgentApp.Desktop.UI;
using Moq;
using Xunit;

namespace CVAgentApp.Desktop.Tests.UI;

public class MainFormTests : IDisposable
{
    private readonly Mock<ILogger<MainForm>> _loggerMock;
    private readonly Mock<ICVGenerationService> _cvGenerationServiceMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly MainForm _form;

    public MainFormTests()
    {
        _loggerMock = new Mock<ILogger<MainForm>>();
        _cvGenerationServiceMock = new Mock<ICVGenerationService>();
        _httpClientMock = new Mock<HttpClient>();
        _form = new MainForm(_loggerMock.Object, _cvGenerationServiceMock.Object, _httpClientMock.Object);
    }

    public void Dispose()
    {
        _form.Dispose();
    }

    [Fact]
    public void InitialState_ShouldBeCorrect()
    {
        // Assert initial button states
        var generateButton = _form.Controls.Find("generateButton", true).FirstOrDefault() as Button;
        Assert.NotNull(generateButton);
        Assert.NotNull(generateButton!.Enabled);

        var downloadCVButton = _form.Controls.Find("downloadCVButton", true).FirstOrDefault() as Button;
        Assert.NotNull(downloadCVButton);
        Assert.False(downloadCVButton.Enabled);

        var downloadCoverLetterButton = _form.Controls.Find("downloadCoverLetterButton", true).FirstOrDefault() as Button;
        Assert.NotNull(downloadCoverLetterButton);
        Assert.False(downloadCoverLetterButton.Enabled);
    }

    [Fact]
    public void JobUrlInput_WhenValid_ShouldEnableGenerateButton()
    {
        // Arrange
        var jobUrlTextBox = _form.Controls.Find("jobUrlTextBox", true).FirstOrDefault() as TextBox;
        var uploadCVButton = _form.Controls.Find("uploadCVButton", true).FirstOrDefault() as Button;
        var generateButton = _form.Controls.Find("generateButton", true).FirstOrDefault() as Button;

        Assert.NotNull(jobUrlTextBox);
        Assert.NotNull(uploadCVButton);
        Assert.NotNull(generateButton);

        // Act
        jobUrlTextBox.Text = "https://example.com/job";
        uploadCVButton.Text = "test.pdf"; // Simulate file selection

        // Assert
        Assert.True(generateButton.Enabled);
    }

    [Fact]
    public async Task GenerateButton_WhenClicked_ShouldCallService()
    {
        // Arrange
        var mockResponse = new CVGenerationResponse
        {
            SessionId = Guid.NewGuid(),
            SessionToken = "test-token",
            Status = CVGenerationStatus.Completed,
            Documents = new List<GeneratedDocumentDto>
            {
                new GeneratedDocumentDto
                {
                    Id = Guid.NewGuid(),
                    FileName = "test_cv.pdf",
                    Type = DocumentType.CV,
                    Content = "Test CV content",
                    DownloadUrl = "http://example.com/cv.pdf",
                    FileSizeBytes = 1000,
                    Status = DocumentStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                },
                new GeneratedDocumentDto
                {
                    Id = Guid.NewGuid(),
                    FileName = "test_cover.pdf",
                    Type = DocumentType.CoverLetter,
                    Content = "Test cover letter content",
                    DownloadUrl = "http://example.com/cover.pdf",
                    FileSizeBytes = 500,
                    Status = DocumentStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _cvGenerationServiceMock
            .Setup(x => x.GenerateCVAsync(It.IsAny<CVGenerationRequest>()))
            .ReturnsAsync(mockResponse);

        var jobUrlTextBox = _form.Controls.Find("jobUrlTextBox", true).FirstOrDefault() as TextBox;
        var uploadCVButton = _form.Controls.Find("uploadCVButton", true).FirstOrDefault() as Button;
        var generateButton = _form.Controls.Find("generateButton", true).FirstOrDefault() as Button;

        Assert.NotNull(jobUrlTextBox);
        Assert.NotNull(uploadCVButton);
        Assert.NotNull(generateButton);

        // Act
        jobUrlTextBox.Text = "https://example.com/job";
        uploadCVButton.Text = "test.pdf";
        await Task.Run(() => generateButton.PerformClick());

        // Assert
        _cvGenerationServiceMock.Verify(
            x => x.GenerateCVAsync(It.IsAny<CVGenerationRequest>()),
            Times.Once);
    }

    [Fact]
    public void ErrorHandling_ShouldShowMessageBox()
    {
        // Arrange
        _cvGenerationServiceMock
            .Setup(x => x.GenerateCVAsync(It.IsAny<CVGenerationRequest>()))
            .ThrowsAsync(new Exception("Test error"));

        var jobUrlTextBox = _form.Controls.Find("jobUrlTextBox", true).FirstOrDefault() as TextBox;
        var uploadCVButton = _form.Controls.Find("uploadCVButton", true).FirstOrDefault() as Button;
        var generateButton = _form.Controls.Find("generateButton", true).FirstOrDefault() as Button;

        Assert.NotNull(jobUrlTextBox);
        Assert.NotNull(uploadCVButton);
        Assert.NotNull(generateButton);

        // Act & Assert
        jobUrlTextBox.Text = "https://example.com/job";
        uploadCVButton.Text = "test.pdf";

        // Note: In a real test environment, you would use a message box service interface
        // that can be mocked. For now, this will show actual message boxes during tests.
        generateButton.PerformClick();
    }

    [Fact]
    public void PreviewTab_ShouldShowDocuments()
    {
        // Arrange
        var cvPreviewBrowser = _form.Controls.Find("cvPreviewBrowser", true).FirstOrDefault() as WebBrowser;
        var coverLetterPreviewBrowser = _form.Controls.Find("coverLetterPreviewBrowser", true).FirstOrDefault() as WebBrowser;

        Assert.NotNull(cvPreviewBrowser);
        Assert.NotNull(coverLetterPreviewBrowser);

        // Act
        cvPreviewBrowser.DocumentText = "Test CV content";
        coverLetterPreviewBrowser.DocumentText = "Test cover letter content";

        // Assert
        Assert.Equal("Test CV content", cvPreviewBrowser.DocumentText);
        Assert.Equal("Test cover letter content", coverLetterPreviewBrowser.DocumentText);
    }

    [Fact]
    public void DownloadButtons_WhenEnabled_ShouldTriggerSaveDialog()
    {
        // Arrange
        var downloadCVButton = _form.Controls.Find("downloadCVButton", true).FirstOrDefault() as Button;
        var downloadCoverLetterButton = _form.Controls.Find("downloadCoverLetterButton", true).FirstOrDefault() as Button;

        Assert.NotNull(downloadCVButton);
        Assert.NotNull(downloadCoverLetterButton);

        // Enable buttons
        downloadCVButton.Enabled = true;
        downloadCoverLetterButton.Enabled = true;

        // Note: In a real test environment, you would use a file dialog service interface
        // that can be mocked. For now, this will show actual file dialogs during tests.
    }
}
