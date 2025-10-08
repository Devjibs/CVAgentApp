using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CVAgentApp.Infrastructure.Services;

/// <summary>
/// Monitoring service implementation for tracking agent performance and system health
/// </summary>
public class MonitoringService : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(ILogger<MonitoringService> logger)
    {
        _logger = logger;
    }

    public async Task LogAgentExecutionAsync(string agentName, Guid sessionId, TimeSpan executionTime, bool success, string? errorMessage = null)
    {
        try
        {
            _logger.LogInformation("Logging agent execution: {AgentName} for session {SessionId}, success: {Success}, execution time: {ExecutionTime}ms",
                agentName, sessionId, success, executionTime.TotalMilliseconds);

            // In a real implementation, this would store the metrics in a database or monitoring system
            var metric = new PerformanceMetric
            {
                MetricName = $"agent_execution_time_{agentName.ToLower()}",
                Value = executionTime.TotalMilliseconds,
                Unit = "milliseconds",
                Timestamp = DateTime.UtcNow,
                Tags = new Dictionary<string, object>
                {
                    ["agent_name"] = agentName,
                    ["session_id"] = sessionId.ToString(),
                    ["success"] = success,
                    ["error_message"] = errorMessage ?? ""
                }
            };

            // Store the metric (mock implementation)
            await StoreMetricAsync(metric);

            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                await LogErrorAsync(agentName, sessionId, errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging agent execution: {AgentName}", agentName);
        }
    }

    public async Task LogGuardrailTriggerAsync(string guardrailName, string violationType, Guid sessionId, string? details = null)
    {
        try
        {
            _logger.LogWarning("Guardrail triggered: {GuardrailName}, violation: {ViolationType}, session: {SessionId}",
                guardrailName, violationType, sessionId);

            var metric = new PerformanceMetric
            {
                MetricName = "guardrail_triggered",
                Value = 1,
                Unit = "count",
                Timestamp = DateTime.UtcNow,
                Tags = new Dictionary<string, object>
                {
                    ["guardrail_name"] = guardrailName,
                    ["violation_type"] = violationType,
                    ["session_id"] = sessionId.ToString(),
                    ["details"] = details ?? ""
                }
            };

            await StoreMetricAsync(metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging guardrail trigger: {GuardrailName}", guardrailName);
        }
    }

    public async Task<SystemHealthReport> GetSystemHealthAsync()
    {
        try
        {
            _logger.LogInformation("Generating system health report");

            var healthChecks = new List<HealthCheck>
            {
                await CheckDatabaseHealthAsync(),
                await CheckOpenAIHealthAsync(),
                await CheckFileStorageHealthAsync(),
                await CheckAgentHealthAsync()
            };

            var overallStatus = healthChecks.All(h => h.Status == HealthStatus.Healthy)
                ? SystemStatus.Healthy
                : healthChecks.Any(h => h.Status == HealthStatus.Unhealthy)
                    ? SystemStatus.Unhealthy
                    : SystemStatus.Degraded;

            var report = new SystemHealthReport
            {
                Status = overallStatus,
                Uptime = CalculateUptime(),
                ActiveSessions = await GetActiveSessionCountAsync(),
                CompletedSessions = await GetCompletedSessionCountAsync(),
                FailedSessions = await GetFailedSessionCountAsync(),
                AverageResponseTime = await GetAverageResponseTimeAsync(),
                HealthChecks = healthChecks,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("System health report generated with status: {Status}", overallStatus);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating system health report");
            throw;
        }
    }

    public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting performance metrics from {FromDate} to {ToDate}", fromDate, toDate);

            // Mock implementation - in real scenario, this would query the metrics database
            var metrics = new List<PerformanceMetric>
            {
                new PerformanceMetric
                {
                    MetricName = "agent_execution_time_cv_parsing",
                    Value = 1250.5,
                    Unit = "milliseconds",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Tags = new Dictionary<string, object>
                    {
                        ["agent_name"] = "CVParsingAgent",
                        ["success"] = true
                    }
                },
                new PerformanceMetric
                {
                    MetricName = "agent_execution_time_job_extraction",
                    Value = 2100.3,
                    Unit = "milliseconds",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Tags = new Dictionary<string, object>
                    {
                        ["agent_name"] = "JobExtractionAgent",
                        ["success"] = true
                    }
                },
                new PerformanceMetric
                {
                    MetricName = "guardrail_triggered",
                    Value = 1,
                    Unit = "count",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Tags = new Dictionary<string, object>
                    {
                        ["guardrail_name"] = "TruthfulnessGuardrail",
                        ["violation_type"] = "FabricatedContent"
                    }
                }
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            throw;
        }
    }

    public async Task<List<ErrorReport>> GetErrorReportsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting error reports from {FromDate} to {ToDate}", fromDate, toDate);

            // Mock implementation - in real scenario, this would query the error logs
            var errors = new List<ErrorReport>
            {
                new ErrorReport
                {
                    Id = Guid.NewGuid(),
                    ErrorType = "AgentExecutionError",
                    Message = "CV parsing agent failed to extract text from PDF",
                    StackTrace = "System.Exception: PDF parsing failed...",
                    SessionId = Guid.NewGuid(),
                    AgentName = "CVParsingAgent",
                    OccurredAt = DateTime.UtcNow.AddHours(-1),
                    Context = new Dictionary<string, object>
                    {
                        ["file_type"] = "application/pdf",
                        ["file_size"] = 1024000
                    }
                },
                new ErrorReport
                {
                    Id = Guid.NewGuid(),
                    ErrorType = "GuardrailViolation",
                    Message = "Truthfulness guardrail detected fabricated content",
                    StackTrace = "",
                    SessionId = Guid.NewGuid(),
                    AgentName = "TruthfulnessGuardrail",
                    OccurredAt = DateTime.UtcNow.AddHours(-2),
                    Context = new Dictionary<string, object>
                    {
                        ["violation_type"] = "FabricatedContent",
                        ["fabricated_skills"] = "Machine Learning, Data Science"
                    }
                }
            };

            return errors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error reports");
            throw;
        }
    }

    private async Task<HealthCheck> CheckDatabaseHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Mock database health check
            await Task.Delay(50);

            var responseTime = DateTime.UtcNow - startTime;

            return new HealthCheck
            {
                Name = "Database",
                Status = HealthStatus.Healthy,
                Message = "Database connection successful",
                ResponseTime = responseTime,
                Details = new Dictionary<string, object>
                {
                    ["connection_pool_size"] = 10,
                    ["active_connections"] = 3
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return new HealthCheck
            {
                Name = "Database",
                Status = HealthStatus.Unhealthy,
                Message = $"Database health check failed: {ex.Message}",
                ResponseTime = TimeSpan.Zero
            };
        }
    }

    private async Task<HealthCheck> CheckOpenAIHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Mock OpenAI health check
            await Task.Delay(100);

            var responseTime = DateTime.UtcNow - startTime;

            return new HealthCheck
            {
                Name = "OpenAI",
                Status = HealthStatus.Healthy,
                Message = "OpenAI API connection successful",
                ResponseTime = responseTime,
                Details = new Dictionary<string, object>
                {
                    ["api_version"] = "v1",
                    ["rate_limit_remaining"] = 1000
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI health check failed");
            return new HealthCheck
            {
                Name = "OpenAI",
                Status = HealthStatus.Unhealthy,
                Message = $"OpenAI health check failed: {ex.Message}",
                ResponseTime = TimeSpan.Zero
            };
        }
    }

    private async Task<HealthCheck> CheckFileStorageHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Mock file storage health check
            await Task.Delay(30);

            var responseTime = DateTime.UtcNow - startTime;

            return new HealthCheck
            {
                Name = "FileStorage",
                Status = HealthStatus.Healthy,
                Message = "File storage accessible",
                ResponseTime = responseTime,
                Details = new Dictionary<string, object>
                {
                    ["storage_type"] = "Local",
                    ["available_space"] = "100GB"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File storage health check failed");
            return new HealthCheck
            {
                Name = "FileStorage",
                Status = HealthStatus.Unhealthy,
                Message = $"File storage health check failed: {ex.Message}",
                ResponseTime = TimeSpan.Zero
            };
        }
    }

    private async Task<HealthCheck> CheckAgentHealthAsync()
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Mock agent health check
            await Task.Delay(20);

            var responseTime = DateTime.UtcNow - startTime;

            return new HealthCheck
            {
                Name = "Agents",
                Status = HealthStatus.Healthy,
                Message = "All agents operational",
                ResponseTime = responseTime,
                Details = new Dictionary<string, object>
                {
                    ["active_agents"] = 5,
                    ["last_execution"] = DateTime.UtcNow.AddMinutes(-5)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent health check failed");
            return new HealthCheck
            {
                Name = "Agents",
                Status = HealthStatus.Unhealthy,
                Message = $"Agent health check failed: {ex.Message}",
                ResponseTime = TimeSpan.Zero
            };
        }
    }

    private double CalculateUptime()
    {
        // Mock implementation - in real scenario, this would calculate actual uptime
        return 99.9;
    }

    private async Task<int> GetActiveSessionCountAsync()
    {
        await Task.Delay(10);
        return 5; // Mock value
    }

    private async Task<int> GetCompletedSessionCountAsync()
    {
        await Task.Delay(10);
        return 150; // Mock value
    }

    private async Task<int> GetFailedSessionCountAsync()
    {
        await Task.Delay(10);
        return 12; // Mock value
    }

    private async Task<double> GetAverageResponseTimeAsync()
    {
        await Task.Delay(10);
        return 2.5; // Mock value in seconds
    }

    private async Task StoreMetricAsync(PerformanceMetric metric)
    {
        // Mock implementation - in real scenario, this would store in a metrics database
        await Task.Delay(10);
        _logger.LogDebug("Stored metric: {MetricName} = {Value} {Unit}", metric.MetricName, metric.Value, metric.Unit);
    }

    private async Task LogErrorAsync(string agentName, Guid sessionId, string errorMessage)
    {
        var errorReport = new ErrorReport
        {
            Id = Guid.NewGuid(),
            ErrorType = "AgentExecutionError",
            Message = errorMessage,
            StackTrace = "", // Would be populated with actual stack trace
            SessionId = sessionId,
            AgentName = agentName,
            OccurredAt = DateTime.UtcNow,
            Context = new Dictionary<string, object>
            {
                ["agent_name"] = agentName,
                ["session_id"] = sessionId.ToString()
            }
        };

        // Store error report (mock implementation)
        await Task.Delay(10);
        _logger.LogError("Error logged: {ErrorType} - {Message}", errorReport.ErrorType, errorReport.Message);
    }
}
