using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CVAgentApp.API.Controllers;

/// <summary>
/// Main CV Agent API controller using multi-agent orchestrator
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CVAgentController : ControllerBase
{
    private readonly ILogger<CVAgentController> _logger;
    private readonly IMultiAgentOrchestrator _orchestrator;
    private readonly ICVGenerationService _cvGenerationService;

    public CVAgentController(
        ILogger<CVAgentController> logger,
        IMultiAgentOrchestrator orchestrator,
        ICVGenerationService cvGenerationService)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _cvGenerationService = cvGenerationService;
    }

    /// <summary>
    /// Generate tailored CV and cover letter using multi-agent workflow
    /// </summary>
    /// <param name="request">CV generation request</param>
    /// <returns>CV generation response</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(CVGenerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CVGenerationResponse>> GenerateCV([FromForm] CVGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Received CV generation request for job: {JobUrl}", request.JobPostingUrl);

            // Validate request
            if (request.CVFile == null || request.CVFile.Length == 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "CV file is required",
                    Code = "MISSING_CV_FILE"
                });
            }

            if (string.IsNullOrEmpty(request.JobPostingUrl))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Job posting URL is required",
                    Code = "MISSING_JOB_URL"
                });
            }

            // Validate file type
            var allowedTypes = new[] { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
            if (!allowedTypes.Contains(request.CVFile.ContentType.ToLower()))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Only PDF and Word documents are supported",
                    Code = "INVALID_FILE_TYPE"
                });
            }

            // Validate file size (max 10MB)
            if (request.CVFile.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "File size must be less than 10MB",
                    Code = "FILE_TOO_LARGE"
                });
            }

            // Execute multi-agent workflow
            var result = await _orchestrator.ExecuteFullWorkflowAsync(request);

            if (!result.Success)
            {
                _logger.LogError("CV generation failed: {Error}", result.ErrorMessage);
                return StatusCode(500, new ErrorResponse
                {
                    Error = result.ErrorMessage ?? "CV generation failed",
                    Code = "GENERATION_FAILED"
                });
            }

            _logger.LogInformation("CV generation completed successfully for session {SessionId}", result.Data?.SessionId);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CV generation request");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An unexpected error occurred",
                Code = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Get the status of a CV generation workflow
    /// </summary>
    /// <param name="sessionToken">Session token</param>
    /// <returns>Session status response</returns>
    [HttpGet("status/{sessionToken}")]
    [ProducesResponseType(typeof(SessionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionStatusResponse>> GetWorkflowStatus(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Getting workflow status for session: {SessionToken}", sessionToken);

            var result = await _orchestrator.GetWorkflowStatusAsync(sessionToken);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "Session not found",
                        Code = "SESSION_NOT_FOUND"
                    });
                }

                return StatusCode(500, new ErrorResponse
                {
                    Error = result.ErrorMessage ?? "Failed to get session status",
                    Code = "STATUS_RETRIEVAL_FAILED"
                });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow status for session: {SessionToken}", sessionToken);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An unexpected error occurred",
                Code = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Cancel a running CV generation workflow
    /// </summary>
    /// <param name="sessionToken">Session token</param>
    /// <returns>Success response</returns>
    [HttpPost("cancel/{sessionToken}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SuccessResponse>> CancelWorkflow(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Cancelling workflow for session: {SessionToken}", sessionToken);

            var result = await _orchestrator.CancelWorkflowAsync(sessionToken);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "Session not found",
                        Code = "SESSION_NOT_FOUND"
                    });
                }

                return StatusCode(500, new ErrorResponse
                {
                    Error = result.ErrorMessage ?? "Failed to cancel workflow",
                    Code = "CANCELLATION_FAILED"
                });
            }

            return Ok(new SuccessResponse
            {
                Message = "Workflow cancelled successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling workflow for session: {SessionToken}", sessionToken);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An unexpected error occurred",
                Code = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Analyze a job posting without generating CV
    /// </summary>
    /// <param name="request">Job analysis request</param>
    /// <returns>Job analysis response</returns>
    [HttpPost("analyze-job")]
    [ProducesResponseType(typeof(JobAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobAnalysisResponse>> AnalyzeJobPosting([FromBody] JobAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing job posting: {JobUrl}", request.JobUrl);

            if (string.IsNullOrEmpty(request.JobUrl))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Job URL is required",
                    Code = "MISSING_JOB_URL"
                });
            }

            var result = await _cvGenerationService.AnalyzeJobPostingAsync(request.JobUrl, request.CompanyName);

            _logger.LogInformation("Job posting analysis completed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing job posting: {JobUrl}", request.JobUrl);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while analyzing the job posting",
                Code = "ANALYSIS_FAILED"
            });
        }
    }

    /// <summary>
    /// Analyze a candidate CV without generating tailored version
    /// </summary>
    /// <param name="cvFile">CV file</param>
    /// <returns>Candidate analysis response</returns>
    [HttpPost("analyze-candidate")]
    [ProducesResponseType(typeof(CandidateAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CandidateAnalysisResponse>> AnalyzeCandidate([FromForm] IFormFile cvFile)
    {
        try
        {
            _logger.LogInformation("Analyzing candidate CV");

            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "CV file is required",
                    Code = "MISSING_CV_FILE"
                });
            }

            // Validate file type
            var allowedTypes = new[] { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
            if (!allowedTypes.Contains(cvFile.ContentType.ToLower()))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Only PDF and Word documents are supported",
                    Code = "INVALID_FILE_TYPE"
                });
            }

            var result = await _cvGenerationService.AnalyzeCandidateAsync(cvFile);

            _logger.LogInformation("Candidate analysis completed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing candidate CV");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while analyzing the CV",
                Code = "ANALYSIS_FAILED"
            });
        }
    }

    /// <summary>
    /// Download a generated document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>Document file</returns>
    [HttpGet("download/{documentId}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadDocument(Guid documentId)
    {
        try
        {
            _logger.LogInformation("Downloading document: {DocumentId}", documentId);

            var documentBytes = await _cvGenerationService.DownloadDocumentAsync(documentId);

            return File(documentBytes, "application/pdf", $"document_{documentId}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {DocumentId}", documentId);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while downloading the document",
                Code = "DOWNLOAD_FAILED"
            });
        }
    }
}

/// <summary>
/// Error response model
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Success response model
/// </summary>
public class SuccessResponse
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
}

