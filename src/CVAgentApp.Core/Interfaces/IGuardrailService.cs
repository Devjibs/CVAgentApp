using CVAgentApp.Core.Entities;
using Microsoft.AspNetCore.Http;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Enums;

namespace CVAgentApp.Core.Interfaces;

/// <summary>
/// Input guardrails - run before agent execution to validate inputs
/// </summary>
public interface IInputGuardrail
{
    Task<GuardrailResult> ValidateAsync(GuardrailContext context);
    string Name { get; }
    int Priority { get; }
}

/// <summary>
/// Output guardrails - run after agent execution to validate outputs
/// </summary>
public interface IOutputGuardrail
{
    Task<GuardrailResult> ValidateAsync(GuardrailContext context);
    string Name { get; }
    int Priority { get; }
}

/// <summary>
/// Guardrail service that manages all guardrails
/// </summary>
public interface IGuardrailService
{
    Task<GuardrailResult> ExecuteInputGuardrailsAsync(GuardrailContext context);
    Task<GuardrailResult> ExecuteOutputGuardrailsAsync(GuardrailContext context);
    Task RegisterInputGuardrailAsync(IInputGuardrail guardrail);
    Task RegisterOutputGuardrailAsync(IOutputGuardrail guardrail);
}

/// <summary>
/// Context for guardrail execution
/// </summary>
public class GuardrailContext
{
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of guardrail execution
/// </summary>
public class GuardrailResult
{
    public bool TripwireTriggered { get; set; }
    public string? ViolationType { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public bool AllowExecution { get; set; } = true;
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Specific guardrails for CV Agent system
/// </summary>

/// <summary>
/// Validates job posting URLs and content
/// </summary>
public interface IJobPostingGuardrail : IInputGuardrail
{
    Task<GuardrailResult> ValidateJobUrlAsync(string jobUrl);
    Task<GuardrailResult> ValidateJobContentAsync(string jobContent);
}

/// <summary>
/// Validates CV files and content
/// </summary>
public interface ICVContentGuardrail : IInputGuardrail
{
    Task<GuardrailResult> ValidateCVFileAsync(IFormFile cvFile);
    Task<GuardrailResult> ValidateCVContentAsync(string cvContent);
}

/// <summary>
/// Prevents hallucination in generated content
/// </summary>
public interface ITruthfulnessGuardrail : IOutputGuardrail
{
    Task<GuardrailResult> ValidateTruthfulnessAsync(string originalContent, string generatedContent);
    Task<GuardrailResult> DetectFabricatedSkillsAsync(string originalCV, string generatedCV);
}

/// <summary>
/// Validates document quality and format
/// </summary>
public interface IDocumentQualityGuardrail : IOutputGuardrail
{
    Task<GuardrailResult> ValidateDocumentQualityAsync(string content, DocumentType type);
    Task<GuardrailResult> ValidateATSCompatibilityAsync(string content);
}

/// <summary>
/// Validates privacy and PII handling
/// </summary>
public interface IPrivacyGuardrail : IInputGuardrail, IOutputGuardrail
{
    Task<GuardrailResult> ValidatePIIHandlingAsync(string content);
    Task<GuardrailResult> ValidateDataRetentionAsync(Dictionary<string, object> metadata);
}

/// <summary>
/// Validates compliance with discrimination laws
/// </summary>
public interface IComplianceGuardrail : IOutputGuardrail
{
    Task<GuardrailResult> ValidateDiscriminationComplianceAsync(string content);
    Task<GuardrailResult> ValidateProtectedAttributesAsync(string content);
}



