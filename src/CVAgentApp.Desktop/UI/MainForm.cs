using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace CVAgentApp.Desktop.UI;

public partial class MainForm : Form
{
    private readonly ILogger<MainForm> _logger;

    public MainForm(ILogger<MainForm> logger)
    {
        _logger = logger;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // Form properties
        this.Text = "CV Agent - AI-Powered CV Generation";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(600, 400);

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
        var statusLabel = new ToolStripStatusLabel("Ready");
        statusStrip.Items.Add(statusLabel);
        this.Controls.Add(statusStrip);

        // Create main content
        var label = new Label();
        label.Text = "CV Agent Desktop Application\n\nThis is a placeholder for the CV Agent application.\nThe full implementation would include:\n\n• AI-powered CV analysis\n• Job posting analysis\n• Tailored CV and cover letter generation\n• Document preview and download\n\nTo complete the implementation, you would need to:\n1. Implement the Core services\n2. Add the Infrastructure layer\n3. Create the web-based UI\n4. Integrate with OpenAI API";
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Font = new Font("Segoe UI", 12);
        this.Controls.Add(label);

        // Load the application
        this.Load += MainForm_Load;
        this.FormClosing += MainForm_FormClosing;

        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("CV Agent Desktop application loaded successfully");
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
