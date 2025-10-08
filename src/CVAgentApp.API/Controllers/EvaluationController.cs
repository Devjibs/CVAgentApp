using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CVAgentApp.API.Controllers;

/// <summary>
/// Evaluation and monitoring API controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EvaluationController : ControllerBase
{
    private readonly ILogger<EvaluationController> _logger;
    private readonly IEvaluationService _evaluationService;
    private readonly IMonitoringService _monitoringService;

    public EvaluationController(
        ILogger<EvaluationController> logger,
        IEvaluationService evaluationService,
        IMonitoringService monitoringService)
    {
        _logger = logger;
        _evaluationService = evaluationService;
        _monitoringService = monitoringService;
    }

    /// <summary>
    /// Evaluate a workflow session
    /// </summary>
    /// <param name="sessionId">Session ID to evaluate</param>
    /// <returns>Evaluation result</returns>
    [HttpPost("workflow/{sessionId}")]
    [ProducesResponseType(typeof(EvaluationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EvaluationResult>> EvaluateWorkflow(Guid sessionId)
    {
        try
        {
            _logger.LogInformation("Evaluating workflow for session: {SessionId}", sessionId);

            var result = await _evaluationService.EvaluateWorkflowAsync(sessionId);

            _logger.LogInformation("Workflow evaluation completed with score: {Score}", result.Score);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating workflow for session: {SessionId}", sessionId);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while evaluating the workflow",
                Code = "EVALUATION_FAILED"
            });
        }
    }

    /// <summary>
    /// Evaluate document quality
    /// </summary>
    /// <param name="request">Document quality evaluation request</param>
    /// <returns>Evaluation result</returns>
    [HttpPost("document-quality")]
    [ProducesResponseType(typeof(EvaluationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EvaluationResult>> EvaluateDocumentQuality([FromBody] DocumentQualityEvaluationRequest request)
    {
        try
        {
            _logger.LogInformation("Evaluating document quality for type: {DocumentType}", request.DocumentType);

            if (string.IsNullOrEmpty(request.Content))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Content is required",
                    Code = "MISSING_CONTENT"
                });
            }

            var result = await _evaluationService.EvaluateDocumentQualityAsync(request.Content, request.DocumentType);

            _logger.LogInformation("Document quality evaluation completed with score: {Score}", result.Score);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating document quality");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while evaluating document quality",
                Code = "EVALUATION_FAILED"
            });
        }
    }

    /// <summary>
    /// Evaluate truthfulness of generated content
    /// </summary>
    /// <param name="request">Truthfulness evaluation request</param>
    /// <returns>Evaluation result</returns>
    [HttpPost("truthfulness")]
    [ProducesResponseType(typeof(EvaluationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EvaluationResult>> EvaluateTruthfulness([FromBody] TruthfulnessEvaluationRequest request)
    {
        try
        {
            _logger.LogInformation("Evaluating truthfulness of generated content");

            if (string.IsNullOrEmpty(request.OriginalContent) || string.IsNullOrEmpty(request.GeneratedContent))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Both original and generated content are required",
                    Code = "MISSING_CONTENT"
                });
            }

            var result = await _evaluationService.EvaluateTruthfulnessAsync(request.OriginalContent, request.GeneratedContent);

            _logger.LogInformation("Truthfulness evaluation completed with score: {Score}", result.Score);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating truthfulness");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while evaluating truthfulness",
                Code = "EVALUATION_FAILED"
            });
        }
    }

    /// <summary>
    /// Evaluate ATS compatibility
    /// </summary>
    /// <param name="request">ATS compatibility evaluation request</param>
    /// <returns>Evaluation result</returns>
    [HttpPost("ats-compatibility")]
    [ProducesResponseType(typeof(EvaluationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EvaluationResult>> EvaluateATSCompatibility([FromBody] ATSCompatibilityEvaluationRequest request)
    {
        try
        {
            _logger.LogInformation("Evaluating ATS compatibility");

            if (string.IsNullOrEmpty(request.Content))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Content is required",
                    Code = "MISSING_CONTENT"
                });
            }

            var result = await _evaluationService.EvaluateATSCompatibilityAsync(request.Content);

            _logger.LogInformation("ATS compatibility evaluation completed with score: {Score}", result.Score);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating ATS compatibility");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while evaluating ATS compatibility",
                Code = "EVALUATION_FAILED"
            });
        }
    }

    /// <summary>
    /// Get evaluation history
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>List of evaluation results</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<EvaluationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<EvaluationResult>>> GetEvaluationHistory(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting evaluation history from {FromDate} to {ToDate}", fromDate, toDate);

            var results = await _evaluationService.GetEvaluationHistoryAsync(fromDate, toDate);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evaluation history");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while retrieving evaluation history",
                Code = "RETRIEVAL_FAILED"
            });
        }
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>Performance metrics</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(EvaluationMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EvaluationMetrics>> GetPerformanceMetrics(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting performance metrics from {FromDate} to {ToDate}", fromDate, toDate);

            var metrics = await _evaluationService.GetPerformanceMetricsAsync(fromDate, toDate);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while retrieving performance metrics",
                Code = "RETRIEVAL_FAILED"
            });
        }
    }

    /// <summary>
    /// Get system health report
    /// </summary>
    /// <returns>System health report</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(SystemHealthReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemHealthReport>> GetSystemHealth()
    {
        try
        {
            _logger.LogInformation("Getting system health report");

            var healthReport = await _monitoringService.GetSystemHealthAsync();

            return Ok(healthReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health report");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while retrieving system health",
                Code = "HEALTH_CHECK_FAILED"
            });
        }
    }

    /// <summary>
    /// Get performance metrics for monitoring
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>List of performance metrics</returns>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(List<PerformanceMetric>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PerformanceMetric>>> GetPerformanceMetrics(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting performance metrics from {FromDate} to {ToDate}", fromDate, toDate);

            var metrics = await _monitoringService.GetPerformanceMetricsAsync(fromDate, toDate);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while retrieving performance metrics",
                Code = "RETRIEVAL_FAILED"
            });
        }
    }

    /// <summary>
    /// Get error reports
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>List of error reports</returns>
    [HttpGet("errors")]
    [ProducesResponseType(typeof(List<ErrorReport>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ErrorReport>>> GetErrorReports(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting error reports from {FromDate} to {ToDate}", fromDate, toDate);

            var errors = await _monitoringService.GetErrorReportsAsync(fromDate, toDate);

            return Ok(errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error reports");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while retrieving error reports",
                Code = "RETRIEVAL_FAILED"
            });
        }
    }
}

/// <summary>
/// Document quality evaluation request
/// </summary>
public class DocumentQualityEvaluationRequest
{
    public string Content { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
}

/// <summary>
/// Truthfulness evaluation request
/// </summary>
public class TruthfulnessEvaluationRequest
{
    public string OriginalContent { get; set; } = string.Empty;
    public string GeneratedContent { get; set; } = string.Empty;
}

/// <summary>
/// ATS compatibility evaluation request
/// </summary>
public class ATSCompatibilityEvaluationRequest
{
    public string Content { get; set; } = string.Empty;
}
