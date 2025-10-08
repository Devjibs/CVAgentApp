using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CVAgentApp.Infrastructure.Guardrails;

/// <summary>
/// Service to register all guardrails with the guardrail service
/// </summary>
public class GuardrailRegistrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GuardrailRegistrationService> _logger;

    public GuardrailRegistrationService(
        IServiceProvider serviceProvider,
        ILogger<GuardrailRegistrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registering guardrails");

            using var scope = _serviceProvider.CreateScope();
            var guardrailService = scope.ServiceProvider.GetRequiredService<IGuardrailService>();

            // Register input guardrails
            var inputGuardrails = scope.ServiceProvider.GetServices<IInputGuardrail>();
            foreach (var guardrail in inputGuardrails)
            {
                await guardrailService.RegisterInputGuardrailAsync(guardrail);
                _logger.LogInformation("Registered input guardrail: {GuardrailName}", guardrail.Name);
            }

            // Register output guardrails
            var outputGuardrails = scope.ServiceProvider.GetServices<IOutputGuardrail>();
            foreach (var guardrail in outputGuardrails)
            {
                await guardrailService.RegisterOutputGuardrailAsync(guardrail);
                _logger.LogInformation("Registered output guardrail: {GuardrailName}", guardrail.Name);
            }

            _logger.LogInformation("All guardrails registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering guardrails");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Guardrail registration service stopped");
        return Task.CompletedTask;
    }
}
