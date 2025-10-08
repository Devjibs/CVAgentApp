using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CVAgentApp.Desktop.UI;
using Microsoft.EntityFrameworkCore;
using CVAgentApp.Core.Interfaces;
using CVAgentApp.Infrastructure.Services;
using CVAgentApp.Infrastructure.Data;
using CVAgentApp.Desktop.Services;

namespace CVAgentApp.Desktop;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            // Create a simple configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            // Create a simple host
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                    });
                    services.AddHttpClient();
                    services.AddSingleton<IConfiguration>(configuration);

                    // Persistence (in-memory DB for desktop runtime)
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("cvagent_desktop"));

                    // Core services - Using mock service for desktop testing
                    services.AddScoped<ICVGenerationService, MockCVGenerationService>();
                    services.AddScoped<IOpenAIService, OpenAIService>();
                    services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
                    services.AddScoped<IFileStorageService, FileStorageService>();
                    services.AddScoped<ISessionService, SessionService>();

                    services.AddTransient<MainForm>();
                })
                .Build();

            // Start the desktop application
            var mainForm = host.Services.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start CV Agent: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}