using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

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
    private readonly IDocumentProcessingService _documentProcessingService;
    private readonly IFileStorageService _fileStorageService;
    
    // In-memory file storage for downloads
    private static readonly ConcurrentDictionary<string, (byte[] Content, string FileName, string ContentType)> _fileCache = new();

    public CVAgentController(
        ILogger<CVAgentController> logger,
        IMultiAgentOrchestrator orchestrator,
        ICVGenerationService cvGenerationService,
        IDocumentProcessingService documentProcessingService,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _cvGenerationService = cvGenerationService;
        _documentProcessingService = documentProcessingService;
        _fileStorageService = fileStorageService;
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
            var allowedTypes = new[] { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/plain" };
            if (!allowedTypes.Contains(request.CVFile.ContentType.ToLower()))
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Only PDF, Word documents, and text files are supported",
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

            // Execute CV generation using the service directly (with mock data)
            _logger.LogInformation("=== CALLING CV GENERATION SERVICE ===");
            Console.WriteLine("=== CALLING CV GENERATION SERVICE ===");
            var result = await _cvGenerationService.GenerateCVAsync(request);
            _logger.LogInformation("=== CV GENERATION SERVICE COMPLETED ===");
            Console.WriteLine("=== CV GENERATION SERVICE COMPLETED ===");

            if (result.Status == CVGenerationStatus.Failed)
            {
                _logger.LogError("CV generation failed: {Error}", result.ErrorMessage);
                return StatusCode(500, new ErrorResponse
                {
                    Error = result.ErrorMessage ?? "CV generation failed",
                    Code = "GENERATION_FAILED",
                    Details = new Dictionary<string, object>{ { "error", result } }
                });
            }

            _logger.LogInformation("CV generation completed successfully for session {SessionId}", result.SessionId);
            
            // Cache the generated documents for download
            var documentsToCache = result.Documents ?? new List<GeneratedDocumentDto>();
            _logger.LogInformation("Documents to cache: {Count}", documentsToCache.Count);
            
            foreach (var doc in documentsToCache)
            {
                try
                {
                    _logger.LogInformation("Caching document: {DocumentId} - {FileName}", doc.Id, doc.FileName);
                    
                    // Generate actual PDF bytes for caching
                    var pdfBytes = await _documentProcessingService.GeneratePDFAsync(doc.Content ?? "");
                    
                    // Cache using both document ID and OpenAI file ID for flexibility
                    var cached1 = _fileCache.TryAdd(doc.Id.ToString(), (pdfBytes, doc.FileName, "application/pdf"));
                    _logger.LogInformation("Cached by ID {DocumentId}: {Success}", doc.Id, cached1);
                    
                    if (!string.IsNullOrEmpty(doc.DownloadUrl))
                    {
                        var cached2 = _fileCache.TryAdd(doc.DownloadUrl, (pdfBytes, doc.FileName, "application/pdf"));
                        _logger.LogInformation("Cached by URL {Url}: {Success}", doc.DownloadUrl, cached2);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error caching document {DocumentId}: {Message}", doc.Id, ex.Message);
                }
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CV generation request");
            return StatusCode(500, new ErrorResponse
            {
                Error = "An unexpected error occurred",
                Code = "INTERNAL_ERROR",
                Details = new Dictionary<string, object>
                {
                    { "ExceptionType", ex.GetType().Name },
                    { "StackTrace", ex.StackTrace }
                }
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
    /// <param name="id">Document ID</param>
    /// <returns>File download</returns>
    [HttpGet("download/{id}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocument(string id)
    {
        try
        {
            _logger.LogInformation("Downloading document: {DocumentId}", id);

            // Check if file exists in memory cache first
            if (_fileCache.TryGetValue(id, out var cachedFile))
            {
                _logger.LogInformation("Serving cached file: {DocumentId}", id);
                return File(cachedFile.Content, cachedFile.ContentType, cachedFile.FileName);
            }

            // If not in cache, return not found
            _logger.LogWarning("Document not found in cache: {DocumentId}", id);
            return NotFound(new ErrorResponse
            {
                Error = "Document not found",
                Code = "DOCUMENT_NOT_FOUND"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {DocumentId}", id);
            return StatusCode(500, new ErrorResponse
            {
                Error = "An unexpected error occurred",
                Code = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Test endpoint to check if routes work
    /// </summary>
    [HttpGet("test/{id}")]
    public IActionResult Test(string id)
    {
        return Ok(new { message = "Test successful", id = id });
    }

        /// <summary>
        /// Test endpoint to check cache status
        /// </summary>
        [HttpGet("cache-status")]
        public IActionResult GetCacheStatus()
        {
            var cacheKeys = _fileCache.Keys.ToList();
            return Ok(new { 
                cacheCount = _fileCache.Count,
                cacheKeys = cacheKeys,
                message = $"Cache contains {_fileCache.Count} items"
            });
        }

        /// <summary>
        /// Test endpoint to manually add something to cache
        /// </summary>
        [HttpGet("test-cache")]
        public async Task<IActionResult> TestCache()
        {
            try
            {
                var testContent = "Test PDF content";
                var pdfBytes = await _documentProcessingService.GeneratePDFAsync(testContent);
                var success = _fileCache.TryAdd("test-doc-id", (pdfBytes, "test.pdf", "application/pdf"));
                
                return Ok(new {
                    message = "Test cache operation completed",
                    success = success,
                    cacheCount = _fileCache.Count,
                    pdfSize = pdfBytes.Length
                });
            }
            catch (Exception ex)
            {
                return Ok(new {
                    message = "Test cache operation failed",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test endpoint to check if service is working
        /// </summary>
        [HttpGet("test-service")]
        public async Task<IActionResult> TestService()
        {
            try
            {
                // Test if the service is working
                var testRequest = new CVGenerationRequest
                {
                    CVFile = null, // This will cause an exception, but we can catch it
                    JobPostingUrl = "https://example.com/test",
                    CompanyName = "Test Company"
                };
                
                return Ok(new { 
                    message = "Service test endpoint reached",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    message = "Service test endpoint reached with exception",
                    exception = ex.Message,
                    timestamp = DateTime.UtcNow
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

