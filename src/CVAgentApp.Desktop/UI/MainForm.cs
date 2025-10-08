using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.Entities;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using CVAgentApp.Desktop.Models;

namespace CVAgentApp.Desktop.UI;

public partial class MainForm : Form
{
    private readonly ILogger<MainForm> _logger;
    private readonly ICVGenerationService _cvGenerationService;
    private readonly HttpClient _httpClient;

    private Panel? mainPanel;
    private TabControl? tabControl;
    private TabPage? uploadTab;
    private TabPage? previewTab;
    private Button? uploadCVButton;
    private TextBox? jobUrlTextBox;
    private TextBox? companyNameTextBox;
    private Button? generateButton;
    private ToolStripStatusLabel? statusLabel;
    private ToolStripProgressBar? progressBar;
    private WebBrowser? cvPreviewBrowser;
    private WebBrowser? coverLetterPreviewBrowser;
    private Button? downloadCVButton;
    private Button? downloadCoverLetterButton;
    private string? sessionToken;
    private string? selectedCvPath;

    public MainForm(ILogger<MainForm> logger, ICVGenerationService cvGenerationService, HttpClient httpClient)
    {
        _logger = logger;
        _cvGenerationService = cvGenerationService;
        _httpClient = httpClient;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // Form properties
        this.Text = "CV Agent - AI-Powered CV Generation";
        this.Size = new Size(1024, 768);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(800, 600);

        // Create menu strip
        var menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("&File");
        var helpMenu = new ToolStripMenuItem("&Help");

        var exitItem = new ToolStripMenuItem("E&xit", null, (s, e) => this.Close());
        var aboutItem = new ToolStripMenuItem("&About", null, ShowAbout);

        fileMenu.DropDownItems.Add(exitItem);
        helpMenu.DropDownItems.Add(aboutItem);

        menuStrip.Items.Add(fileMenu);
        menuStrip.Items.Add(helpMenu);

        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);

        // Create status strip
        var statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("Ready");
        progressBar = new ToolStripProgressBar { Width = 200, Visible = false };
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        this.Controls.Add(statusStrip);

        // Create main panel
        mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        this.Controls.Add(mainPanel);

        // Create tab control
        tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };
        mainPanel.Controls.Add(tabControl);

        // Create Upload tab
        uploadTab = new TabPage("Upload & Generate");
        tabControl.TabPages.Add(uploadTab);

        var uploadPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 4
        };
        uploadPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        uploadPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        uploadTab.Controls.Add(uploadPanel);

        // CV Upload section
        var cvLabel = new Label
        {
            Text = "Upload your CV:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        uploadPanel.Controls.Add(cvLabel, 0, 0);

        var cvButtonPanel = new Panel { Dock = DockStyle.Fill };
        uploadCVButton = new Button
        {
            Text = "Choose File",
            Width = 120,
            Height = 30
        };
        uploadCVButton.Click += UploadCV_Click;
        cvButtonPanel.Controls.Add(uploadCVButton);
        uploadPanel.Controls.Add(cvButtonPanel, 1, 0);

        // Job URL section
        var jobUrlLabel = new Label
        {
            Text = "Job Posting URL:",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        uploadPanel.Controls.Add(jobUrlLabel, 0, 1);

        jobUrlTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Enter the job posting URL"
        };
        // Enable Generate when URL and CV are provided
        jobUrlTextBox.TextChanged += (s, e) =>
        {
            if (generateButton != null)
            {
                var hasUrl = !string.IsNullOrWhiteSpace(jobUrlTextBox.Text);
                generateButton.Enabled = hasUrl && !string.IsNullOrEmpty(selectedCvPath);
            }
        };
        uploadPanel.Controls.Add(jobUrlTextBox, 1, 1);

        // Company Name section
        var companyLabel = new Label
        {
            Text = "Company Name (optional):",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        uploadPanel.Controls.Add(companyLabel, 0, 2);

        companyNameTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Enter company name (optional)"
        };
        uploadPanel.Controls.Add(companyNameTextBox, 1, 2);

        // Generate button section
        var generatePanel = new Panel { Dock = DockStyle.Fill };
        generateButton = new Button
        {
            Text = "Generate CV & Cover Letter",
            Width = 200,
            Height = 40,
            Enabled = false
        };
        generateButton.Click += Generate_Click;
        generatePanel.Controls.Add(generateButton);
        uploadPanel.Controls.Add(generatePanel, 1, 3);

        // Create Preview tab
        previewTab = new TabPage("Preview & Download");
        tabControl.TabPages.Add(previewTab);

        var previewPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 2
        };
        previewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        previewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        previewTab.Controls.Add(previewPanel);

        // CV Preview section
        var cvPreviewPanel = new Panel { Dock = DockStyle.Fill };
        var cvPreviewLabel = new Label
        {
            Text = "CV Preview",
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 30
        };
        cvPreviewPanel.Controls.Add(cvPreviewLabel);

        cvPreviewBrowser = new WebBrowser
        {
            Dock = DockStyle.Fill
        };
        cvPreviewPanel.Controls.Add(cvPreviewBrowser);

        downloadCVButton = new Button
        {
            Text = "Download CV",
            Dock = DockStyle.Bottom,
            Height = 30,
            Enabled = false
        };
        downloadCVButton.Click += DownloadCV_Click;
        cvPreviewPanel.Controls.Add(downloadCVButton);

        previewPanel.Controls.Add(cvPreviewPanel, 0, 0);

        // Cover Letter Preview section
        var coverLetterPreviewPanel = new Panel { Dock = DockStyle.Fill };
        var coverLetterPreviewLabel = new Label
        {
            Text = "Cover Letter Preview",
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 30
        };
        coverLetterPreviewPanel.Controls.Add(coverLetterPreviewLabel);

        coverLetterPreviewBrowser = new WebBrowser
        {
            Dock = DockStyle.Fill
        };
        coverLetterPreviewPanel.Controls.Add(coverLetterPreviewBrowser);

        downloadCoverLetterButton = new Button
        {
            Text = "Download Cover Letter",
            Dock = DockStyle.Bottom,
            Height = 30,
            Enabled = false
        };
        downloadCoverLetterButton.Click += DownloadCoverLetter_Click;
        coverLetterPreviewPanel.Controls.Add(downloadCoverLetterButton);

        previewPanel.Controls.Add(coverLetterPreviewPanel, 1, 0);

        // Load the application
        this.Load += MainForm_Load;
        this.FormClosing += MainForm_FormClosing;

        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void UploadCV_Click(object? sender, EventArgs e)
    {
        try
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "CV Files|*.pdf;*.doc;*.docx|All Files|*.*",
                Title = "Select your CV"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK && uploadCVButton != null && generateButton != null)
            {
                selectedCvPath = openFileDialog.FileName;
                uploadCVButton.Text = Path.GetFileName(openFileDialog.FileName);
                var hasUrl = !string.IsNullOrWhiteSpace(jobUrlTextBox?.Text);
                generateButton.Enabled = hasUrl && !string.IsNullOrEmpty(selectedCvPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CV");
            MessageBox.Show("Error uploading CV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void Generate_Click(object? sender, EventArgs e)
    {
        try
        {
            if (generateButton == null || uploadCVButton == null || jobUrlTextBox == null || 
                companyNameTextBox == null || progressBar == null || statusLabel == null)
            {
                MessageBox.Show("UI controls not initialized properly", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Disable controls
            generateButton.Enabled = false;
            uploadCVButton.Enabled = false;
            jobUrlTextBox.Enabled = false;
            companyNameTextBox.Enabled = false;
            progressBar.Visible = true;
            statusLabel.Text = "Generating documents...";

            // Create the request
            if (string.IsNullOrEmpty(selectedCvPath))
            {
                MessageBox.Show("Please choose a CV file first.", "CV required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cvFile = await File.ReadAllBytesAsync(selectedCvPath);
            var request = new CVGenerationRequest
            {
                CVFile = new FileWrapper(new MemoryStream(cvFile), Path.GetFileName(selectedCvPath)),
                JobPostingUrl = jobUrlTextBox.Text,
                CompanyName = string.IsNullOrWhiteSpace(companyNameTextBox.Text) ? null : companyNameTextBox.Text
            };

            // Generate documents
            var response = await _cvGenerationService.GenerateCVAsync(request);
            sessionToken = response.SessionToken;

            // Update preview
            if (response.Documents.Count >= 2)
            {
                var cvDoc = response.Documents.First(d => d.Type == DocumentType.CV);
                var coverLetterDoc = response.Documents.First(d => d.Type == DocumentType.CoverLetter);

                if (cvPreviewBrowser != null) cvPreviewBrowser.DocumentText = cvDoc.Content;
                if (coverLetterPreviewBrowser != null) coverLetterPreviewBrowser.DocumentText = coverLetterDoc.Content;

                if (downloadCVButton != null) downloadCVButton.Enabled = true;
                if (downloadCoverLetterButton != null) downloadCoverLetterButton.Enabled = true;

                if (tabControl != null && previewTab != null) tabControl.SelectedTab = previewTab;
            }

            if (statusLabel != null) statusLabel.Text = "Documents generated successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documents");
            MessageBox.Show("Error generating documents: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            statusLabel.Text = "Error generating documents";
        }
        finally
        {
            // Re-enable controls
            if (generateButton != null) generateButton.Enabled = true;
            if (uploadCVButton != null) uploadCVButton.Enabled = true;
            if (jobUrlTextBox != null) jobUrlTextBox.Enabled = true;
            if (companyNameTextBox != null) companyNameTextBox.Enabled = true;
            if (progressBar != null) progressBar.Visible = false;
        }
    }

    private async void DownloadCV_Click(object? sender, EventArgs e)
    {
        try
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Save CV",
                FileName = $"{GetCandidateName()}_CV.pdf"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (statusLabel != null) statusLabel.Text = "Downloading CV...";
                if (progressBar != null) progressBar.Visible = true;

                var response = await _cvGenerationService.GetSessionStatusAsync(sessionToken!);
                var cvDoc = response.Documents.First(d => d.Type == DocumentType.CV);
                
                if (!string.IsNullOrEmpty(cvDoc.DownloadUrl))
                {
                    var bytes = await _httpClient.GetByteArrayAsync(cvDoc.DownloadUrl);
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, bytes);
                    if (statusLabel != null) statusLabel.Text = "CV downloaded successfully";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading CV");
            MessageBox.Show("Error downloading CV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (statusLabel != null) statusLabel.Text = "Error downloading CV";
        }
        finally
        {
            if (progressBar != null) progressBar.Visible = false;
        }
    }

    private async void DownloadCoverLetter_Click(object? sender, EventArgs e)
    {
        try
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Save Cover Letter",
                FileName = $"{GetCandidateName()}_CoverLetter.pdf"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (statusLabel != null) statusLabel.Text = "Downloading cover letter...";
                if (progressBar != null) progressBar.Visible = true;

                var response = await _cvGenerationService.GetSessionStatusAsync(sessionToken!);
                var coverLetterDoc = response.Documents.First(d => d.Type == DocumentType.CoverLetter);
                
                if (!string.IsNullOrEmpty(coverLetterDoc.DownloadUrl))
                {
                    var bytes = await _httpClient.GetByteArrayAsync(coverLetterDoc.DownloadUrl);
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, bytes);
                    if (statusLabel != null) statusLabel.Text = "Cover letter downloaded successfully";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading cover letter");
            MessageBox.Show("Error downloading cover letter: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (statusLabel != null) statusLabel.Text = "Error downloading cover letter";
        }
        finally
        {
            if (progressBar != null) progressBar.Visible = false;
        }
    }

    private string GetCandidateName()
    {
        // In a real implementation, this would come from the parsed CV or user profile
        return "Candidate";
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("CV Agent Desktop application loaded successfully");
            if (statusLabel != null) statusLabel.Text = "Ready";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load CV Agent Desktop application");
            MessageBox.Show($"Failed to load application: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowAbout(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "CV Agent Desktop v1.0\n\n" +
            "AI-Powered CV Generation Application\n\n" +
            "Transform your CV with AI to match any job posting perfectly.\n\n" +
            "Features:\n" +
            "• AI-powered CV analysis\n" +
            "• Job posting analysis\n" +
            "• Tailored CV and cover letter generation\n" +
            "• Company research integration\n" +
            "• Document preview and download\n\n" +
            "© 2024 CV Agent",
            "About CV Agent",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        try
        {
            _logger.LogInformation("CV Agent Desktop application closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application shutdown");
        }
    }
}