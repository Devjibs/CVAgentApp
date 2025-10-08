using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CVAgentApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CVGenerationController : ControllerBase
{
    private readonly ILogger<CVGenerationController> _logger;
    private readonly ICVGenerationService _cvGenerationService;

    public CVGenerationController(ILogger<CVGenerationController> logger, ICVGenerationService cvGenerationService)
    {
        _logger = logger;
        _cvGenerationService = cvGenerationService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<CVGenerationResponse>> GenerateCV([FromForm] CVGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Received CV generation request");

            if (request.CVFile == null || request.CVFile.Length == 0)
            {
                return BadRequest("CV file is required");
            }

            if (string.IsNullOrEmpty(request.JobPostingUrl))
            {
                return BadRequest("Job posting URL is required");
            }

            var result = await _cvGenerationService.GenerateCVAsync(request);

            _logger.LogInformation("CV generation request processed: {Status}", result.Status);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CV generation request");
            return StatusCode(500, new CVGenerationResponse
            {
                Status = CVGenerationStatus.Failed,
                ErrorMessage = "An error occurred while processing your request"
            });
        }
    }

    [HttpGet("session/{sessionToken}")]
    public async Task<ActionResult<SessionStatusResponse>> GetSessionStatus(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Getting session status: {SessionToken}", sessionToken);

            var result = await _cvGenerationService.GetSessionStatusAsync(sessionToken);

            if (result.Status == CVGenerationStatus.Failed)
            {
                return NotFound("Session not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status: {SessionToken}", sessionToken);
            return StatusCode(500, "An error occurred while retrieving session status");
        }
    }

    [HttpPost("analyze-candidate")]
    public async Task<ActionResult<CandidateAnalysisResponse>> AnalyzeCandidate([FromForm] IFormFile cvFile)
    {
        try
        {
            _logger.LogInformation("Analyzing candidate CV");

            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest("CV file is required");
            }

            var result = await _cvGenerationService.AnalyzeCandidateAsync(cvFile);

            _logger.LogInformation("Candidate analysis completed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing candidate CV");
            return StatusCode(500, "An error occurred while analyzing the CV");
        }
    }

    [HttpPost("analyze-job")]
    public async Task<ActionResult<JobAnalysisResponse>> AnalyzeJobPosting([FromBody] JobAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing job posting: {JobUrl}", request.JobUrl);

            if (string.IsNullOrEmpty(request.JobUrl))
            {
                return BadRequest("Job URL is required");
            }

            var result = await _cvGenerationService.AnalyzeJobPostingAsync(request.JobUrl, request.CompanyName);

            _logger.LogInformation("Job posting analysis completed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing job posting: {JobUrl}", request.JobUrl);
            return StatusCode(500, "An error occurred while analyzing the job posting");
        }
    }

    [HttpGet("download/{documentId}")]
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
            return StatusCode(500, "An error occurred while downloading the document");
        }
    }

    [HttpDelete("session/{sessionToken}")]
    public async Task<ActionResult> DeleteSession(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Deleting session: {SessionToken}", sessionToken);

            var result = await _cvGenerationService.DeleteSessionAsync(sessionToken);

            if (!result)
            {
                return NotFound("Session not found");
            }

            return Ok(new { message = "Session deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session: {SessionToken}", sessionToken);
            return StatusCode(500, "An error occurred while deleting the session");
        }
    }
}

public class JobAnalysisRequest
{
    public string JobUrl { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
}
