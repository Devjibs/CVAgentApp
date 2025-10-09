using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CVAgentApp.Infrastructure.Guardrails;

/// <summary>
/// Guardrail service implementation that manages all guardrails
/// </summary>
public class GuardrailService : IGuardrailService
{
    private readonly ILogger<GuardrailService> _logger;
    private readonly List<IInputGuardrail> _inputGuardrails;
    private readonly List<IOutputGuardrail> _outputGuardrails;

    public GuardrailService(ILogger<GuardrailService> logger)
    {
        _logger = logger;
        _inputGuardrails = new List<IInputGuardrail>();
        _outputGuardrails = new List<IOutputGuardrail>();
    }

    public async Task<GuardrailResult> ExecuteInputGuardrailsAsync(GuardrailContext context)
    {
        try
        {
            _logger.LogInformation("Executing input guardrails for agent {AgentName}", context.AgentName);

            var results = new List<GuardrailResult>();

            // Execute all input guardrails in parallel
            var tasks = _inputGuardrails.Select(async guardrail =>
            {
                try
                {
                    return await guardrail.ValidateAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing input guardrail {GuardrailName}", guardrail.Name);
                    return new GuardrailResult
                    {
                        TripwireTriggered = true,
                        ViolationType = "GuardrailError",
                        Message = $"Error in guardrail {guardrail.Name}: {ex.Message}",
                        AllowExecution = false
                    };
                }
            });

            results.AddRange(await Task.WhenAll(tasks));

            // Check if any guardrail triggered
            var triggeredGuardrails = results.Where(r => r.TripwireTriggered).ToList();
            if (triggeredGuardrails.Any())
            {
                var violationTypes = string.Join(", ", triggeredGuardrails.Select(r => r.ViolationType));
                var messages = string.Join("; ", triggeredGuardrails.Select(r => r.Message));

                _logger.LogWarning("Input guardrails triggered: {ViolationTypes} - {Messages}", violationTypes, messages);

                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = violationTypes,
                    Message = messages,
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["triggeredGuardrails"] = triggeredGuardrails.Count,
                        ["violationTypes"] = violationTypes
                    }
                };
            }

            _logger.LogInformation("All input guardrails passed for agent {AgentName}", context.AgentName);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing input guardrails");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "SystemError",
                Message = "System error during guardrail execution",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ExecuteOutputGuardrailsAsync(GuardrailContext context)
    {
        try
        {
            _logger.LogInformation("Executing output guardrails for agent {AgentName}", context.AgentName);

            var results = new List<GuardrailResult>();

            // Execute all output guardrails in parallel
            var tasks = _outputGuardrails.Select(async guardrail =>
            {
                try
                {
                    return await guardrail.ValidateAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing output guardrail {GuardrailName}", guardrail.Name);
                    return new GuardrailResult
                    {
                        TripwireTriggered = true,
                        ViolationType = "GuardrailError",
                        Message = $"Error in guardrail {guardrail.Name}: {ex.Message}",
                        AllowExecution = false
                    };
                }
            });

            results.AddRange(await Task.WhenAll(tasks));

            // Check if any guardrail triggered
            var triggeredGuardrails = results.Where(r => r.TripwireTriggered).ToList();
            if (triggeredGuardrails.Any())
            {
                var violationTypes = string.Join(", ", triggeredGuardrails.Select(r => r.ViolationType));
                var messages = string.Join("; ", triggeredGuardrails.Select(r => r.Message));

                _logger.LogWarning("Output guardrails triggered: {ViolationTypes} - {Messages}", violationTypes, messages);

                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = violationTypes,
                    Message = messages,
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["triggeredGuardrails"] = triggeredGuardrails.Count,
                        ["violationTypes"] = violationTypes
                    }
                };
            }

            _logger.LogInformation("All output guardrails passed for agent {AgentName}", context.AgentName);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing output guardrails");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "SystemError",
                Message = "System error during guardrail execution",
                AllowExecution = false
            };
        }
    }

    public async Task RegisterInputGuardrailAsync(IInputGuardrail guardrail)
    {
        _logger.LogInformation("Registering input guardrail: {GuardrailName}", guardrail.Name);
        _inputGuardrails.Add(guardrail);
        await Task.CompletedTask;
    }

    public async Task RegisterOutputGuardrailAsync(IOutputGuardrail guardrail)
    {
        _logger.LogInformation("Registering output guardrail: {GuardrailName}", guardrail.Name);
        _outputGuardrails.Add(guardrail);
        await Task.CompletedTask;
    }
}



