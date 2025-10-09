using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using CVAgentApp.Core.Enums;

namespace CVAgentApp.Core.Interfaces;

/// <summary>
/// Evaluation service for monitoring and improving agent performance
/// </summary>
public interface IEvaluationService
{
    Task<EvaluationResult> EvaluateWorkflowAsync(Guid sessionId);
    Task<EvaluationResult> EvaluateDocumentQualityAsync(string content, DocumentType type);
    Task<EvaluationResult> EvaluateTruthfulnessAsync(string originalContent, string generatedContent);
    Task<EvaluationResult> EvaluateATSCompatibilityAsync(string content);
    Task<List<EvaluationResult>> GetEvaluationHistoryAsync(DateTime fromDate, DateTime toDate);
    Task<EvaluationMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate);
}

/// <summary>
/// Monitoring service for tracking agent performance and system health
/// </summary>
public interface IMonitoringService
{
    Task LogAgentExecutionAsync(string agentName, Guid sessionId, TimeSpan executionTime, bool success, string? errorMessage = null);
    Task LogGuardrailTriggerAsync(string guardrailName, string violationType, Guid sessionId, string? details = null);
    Task<SystemHealthReport> GetSystemHealthAsync();
    Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate);
    Task<List<ErrorReport>> GetErrorReportsAsync(DateTime fromDate, DateTime toDate);
}

/// <summary>
/// Result of an evaluation
/// </summary>
public class EvaluationResult
{
    public Guid Id { get; set; }
    public string EvaluationType { get; set; } = string.Empty;
    public Guid SessionId { get; set; }
    public double Score { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    public bool Passed { get; set; }
}

/// <summary>
/// Performance metrics for the system
/// </summary>
public class EvaluationMetrics
{
    public double AverageScore { get; set; }
    public double SuccessRate { get; set; }
    public double AverageExecutionTime { get; set; }
    public int TotalEvaluations { get; set; }
    public int PassedEvaluations { get; set; }
    public int FailedEvaluations { get; set; }
    public Dictionary<string, double> ScoreByType { get; set; } = new();
    public List<string> CommonIssues { get; set; } = new();
    public List<string> TopRecommendations { get; set; } = new();
}

/// <summary>
/// System health report
/// </summary>
public class SystemHealthReport
{
    public SystemStatus Status { get; set; }
    public double Uptime { get; set; }
    public int ActiveSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int FailedSessions { get; set; }
    public double AverageResponseTime { get; set; }
    public List<HealthCheck> HealthChecks { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual health check result
/// </summary>
public class HealthCheck
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Performance metric
/// </summary>
public class PerformanceMetric
{
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Tags { get; set; } = new();
}

/// <summary>
/// Error report
/// </summary>
public class ErrorReport
{
    public Guid Id { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public string? AgentName { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Context { get; set; } = new();
}

public enum SystemStatus
{
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3,
    Unknown = 4
}

public enum HealthStatus
{
    Healthy = 1,
    Unhealthy = 2,
    Unknown = 3
}



