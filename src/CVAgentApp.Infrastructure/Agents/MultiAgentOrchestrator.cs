using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CVAgentApp.Core.Enums;
using System.Linq;

namespace CVAgentApp.Infrastructure.Agents;

/// <summary>
/// Multi-Agent Orchestrator that coordinates all agents in the CV generation workflow
/// </summary>
public class MultiAgentOrchestrator : IMultiAgentOrchestrator
{
    private readonly ILogger<MultiAgentOrchestrator> _logger;
    private readonly ICVParsingAgent _cvParsingAgent;
    private readonly IJobExtractionAgent _jobExtractionAgent;
    private readonly IMatchingAgent _matchingAgent;
    private readonly ICVGenerationAgent _cvGenerationAgent;
    private readonly IReviewAgent _reviewAgent;
    private readonly ISessionService _sessionService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentProcessingService _documentProcessor;

    public MultiAgentOrchestrator(
        ILogger<MultiAgentOrchestrator> logger,
        ICVParsingAgent cvParsingAgent,
        IJobExtractionAgent jobExtractionAgent,
        IMatchingAgent matchingAgent,
        ICVGenerationAgent cvGenerationAgent,
        IReviewAgent reviewAgent,
        ISessionService sessionService,
        IFileStorageService fileStorageService,
        IDocumentProcessingService documentProcessor)
    {
        _logger = logger;
        _cvParsingAgent = cvParsingAgent;
        _jobExtractionAgent = jobExtractionAgent;
        _matchingAgent = matchingAgent;
        _cvGenerationAgent = cvGenerationAgent;
        _reviewAgent = reviewAgent;
        _sessionService = sessionService;
        _fileStorageService = fileStorageService;
        _documentProcessor = documentProcessor;
    }

    public async Task<AgentResult<CVGenerationResponse>> ExecuteFullWorkflowAsync(CVGenerationRequest request)
    {
        var sessionId = Guid.NewGuid();
        var sessionToken = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation("Starting full workflow for session {SessionId}", sessionId);

            // Create session
            var session = await _sessionService.CreateSessionAsync(Guid.NewGuid(), Guid.NewGuid());
            if (session == null)
            {
                return new AgentResult<CVGenerationResponse>
                {
                    Success = false,
                    ErrorMessage = "Failed to create session"
                };
            }

            // Create agent context
            var context = new AgentContext
            {
                SessionId = sessionId,
                SessionToken = sessionToken,
                UserId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>
                {
                    ["cvFile"] = request.CVFile,
                    ["jobUrl"] = request.JobPostingUrl,
                    ["companyName"] = request.CompanyName ?? ""
                }
            };

            // Step 1: Parse CV
            _logger.LogInformation("Step 1: Parsing CV");
            var cvParsingResult = await _cvParsingAgent.ParseCVAsync(request.CVFile, context);
            if (!cvParsingResult.Success)
            {
                await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Failed, "CV parsing failed");
                return new AgentResult<CVGenerationResponse>
                {
                    Success = false,
                    ErrorMessage = cvParsingResult.ErrorMessage
                };
            }

            context.Metadata["candidate"] = cvParsingResult.Data;
            await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Processing, "CV parsed successfully");

            // Step 2: Extract Job Posting
            _logger.LogInformation("Step 2: Extracting job posting");
            var jobExtractionResult = await _jobExtractionAgent.ExtractJobPostingAsync(
                request.JobPostingUrl,
                request.CompanyName,
                context);

            if (!jobExtractionResult.Success)
            {
                await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Failed, "Job extraction failed");
                return new AgentResult<CVGenerationResponse>
                {
                    Success = false,
                    ErrorMessage = jobExtractionResult.ErrorMessage
                };
            }

            context.Metadata["job"] = jobExtractionResult.Data;
            await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Processing, "Job posting extracted successfully");

            // Step 3: Match Candidate to Job
            _logger.LogInformation("Step 3: Matching candidate to job");
            var matchingResult = await _matchingAgent.MatchCandidateToJobAsync(
                cvParsingResult.Data!,
                jobExtractionResult.Data!,
                context);

            if (!matchingResult.Success)
            {
                await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Failed, "Matching failed");
                return new AgentResult<CVGenerationResponse>
                {
                    Success = false,
                    ErrorMessage = matchingResult.ErrorMessage
                };
            }

            context.Metadata["matching"] = matchingResult.Data;
            await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Processing, "Matching completed successfully");

            // Step 4: Generate Tailored CV
            _logger.LogInformation("Step 4: Generating tailored CV");
            var cvGenerationResult = await _cvGenerationAgent.GenerateTailoredCVAsync(
                cvParsingResult.Data!,
                jobExtractionResult.Data!,
                matchingResult.Data!,
                context);

            if (!cvGenerationResult.Success)
            {
                await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Failed, "CV generation failed");
                return new AgentResult<CVGenerationResponse>
                {
                    Success = false,
                    ErrorMessage = cvGenerationResult.ErrorMessage
                };
            }

            // Step 5: Generate Cover Letter
            _logger.LogInformation("Step 5: Generating cover letter");
            var coverLetterResult = await _cvGenerationAgent.GenerateCoverLetterAsync(
                cvParsingResult.Data!,
                jobExtractionResult.Data!,
                matchingResult.Data!,
                context);

            if (!coverLetterResult.Success)
            {
                await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Failed, "Cover letter generation failed");
                return new AgentResult<CVGenerationResponse>
                {
                    Success = false,
                    ErrorMessage = coverLetterResult.ErrorMessage
                };
            }

            // Step 6: Review Documents
            _logger.LogInformation("Step 6: Reviewing documents");
            context.Metadata["content"] = cvGenerationResult.Data;
            context.Metadata["documentType"] = DocumentType.CV;

            var cvReviewResult = await _reviewAgent.ReviewDocumentAsync(
                cvGenerationResult.Data!,
                DocumentType.CV,
                context);

            context.Metadata["content"] = coverLetterResult.Data;
            context.Metadata["documentType"] = DocumentType.CoverLetter;

            var coverLetterReviewResult = await _reviewAgent.ReviewDocumentAsync(
                coverLetterResult.Data!,
                DocumentType.CoverLetter,
                context);

            // Step 7: Format and Store Documents
            _logger.LogInformation("Step 7: Formatting and storing documents");
            var documents = new List<GeneratedDocumentDto>();

            // Format CV
            var cvFormatResult = await _cvGenerationAgent.FormatDocumentAsync(
                cvGenerationResult.Data!,
                DocumentType.CV,
                context);

            if (cvFormatResult.Success)
            {
                var cvBlobUrl = await _fileStorageService.UploadFileAsync(
                    new MemoryStream(cvFormatResult.Data!),
                    $"CV_{cvParsingResult.Data!.FirstName}_{cvParsingResult.Data.LastName}_{jobExtractionResult.Data!.JobTitle}.pdf",
                    "application/pdf");

                documents.Add(new GeneratedDocumentDto
                {
                    Id = Guid.NewGuid(),
                    FileName = $"CV_{cvParsingResult.Data.FirstName}_{cvParsingResult.Data.LastName}_{jobExtractionResult.Data.JobTitle}.pdf",
                    Type = DocumentType.CV,
                    Content = cvGenerationResult.Data!,
                    DownloadUrl = cvBlobUrl,
                    FileSizeBytes = cvFormatResult.Data!.Length,
                    Status = DocumentStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Format Cover Letter
            var coverLetterFormatResult = await _cvGenerationAgent.FormatDocumentAsync(
                coverLetterResult.Data!,
                DocumentType.CoverLetter,
                context);

            if (coverLetterFormatResult.Success)
            {
                var coverLetterBlobUrl = await _fileStorageService.UploadFileAsync(
                    new MemoryStream(coverLetterFormatResult.Data!),
                    $"CoverLetter_{cvParsingResult.Data!.FirstName}_{cvParsingResult.Data.LastName}_{jobExtractionResult.Data!.JobTitle}.pdf",
                    "application/pdf");

                documents.Add(new GeneratedDocumentDto
                {
                    Id = Guid.NewGuid(),
                    FileName = $"CoverLetter_{cvParsingResult.Data.FirstName}_{cvParsingResult.Data.LastName}_{jobExtractionResult.Data.JobTitle}.pdf",
                    Type = DocumentType.CoverLetter,
                    Content = coverLetterResult.Data!,
                    DownloadUrl = coverLetterBlobUrl,
                    FileSizeBytes = coverLetterFormatResult.Data!.Length,
                    Status = DocumentStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Complete session
            await _sessionService.CompleteSessionAsync(session.Id);

            _logger.LogInformation("Full workflow completed successfully for session {SessionId}", sessionId);

            return new AgentResult<CVGenerationResponse>
            {
                Success = true,
                Data = new CVGenerationResponse
                {
                    SessionId = sessionId,
                    SessionToken = sessionToken,
                    Status = CVGenerationStatus.Completed,
                    Message = "CV and cover letter generated successfully",
                    Documents = documents
                },
                Metadata = new Dictionary<string, object>
                {
                    ["matchScore"] = matchingResult.Data!.MatchScore,
                    ["cvQualityScore"] = cvReviewResult.Data?.QualityScore ?? 0,
                    ["coverLetterQualityScore"] = coverLetterReviewResult.Data?.QualityScore ?? 0,
                    ["documentsGenerated"] = documents.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing full workflow for session {SessionId}", sessionId);
            return new AgentResult<CVGenerationResponse>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<SessionStatusResponse>> GetWorkflowStatusAsync(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Getting workflow status for session token: {SessionToken}", sessionToken);

            var session = await _sessionService.GetSessionAsync(sessionToken);
            if (session == null)
            {
                return new AgentResult<SessionStatusResponse>
                {
                    Success = false,
                    ErrorMessage = "Session not found"
                };
            }

            var documents = session.GeneratedDocuments.Select(d => new GeneratedDocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                Type = d.Type,
                Content = d.Content,
                DownloadUrl = d.BlobUrl,
                FileSizeBytes = d.FileSizeBytes,
                Status = d.Status,
                CreatedAt = d.CreatedAt
            }).ToList();

            return new AgentResult<SessionStatusResponse>
            {
                Success = true,
                Data = new SessionStatusResponse
                {
                    SessionId = session.Id,
                    SessionToken = session.SessionToken,
                    Status = (CVGenerationStatus)session.Status,
                    ProcessingLog = session.ProcessingLog,
                    Documents = documents,
                    CreatedAt = session.CreatedAt,
                    CompletedAt = session.CompletedAt,
                    ExpiresAt = session.ExpiresAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow status");
            return new AgentResult<SessionStatusResponse>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<CancellationResult>> CancelWorkflowAsync(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Cancelling workflow for session token: {SessionToken}", sessionToken);

            var session = await _sessionService.GetSessionAsync(sessionToken);
            if (session == null)
            {
            return new AgentResult<CancellationResult>
            {
                Success = false,
                ErrorMessage = "Session not found"
            };
            }

            var result = await _sessionService.UpdateSessionStatusAsync(session.Id, SessionStatus.Failed, "Workflow cancelled by user");

            return new AgentResult<CancellationResult>
            {
                Success = result,
                Data = new CancellationResult
                {
                    WasCancelled = result,
                    Message = result ? "Workflow cancelled successfully" : "Failed to cancel workflow"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling workflow");
            return new AgentResult<CancellationResult>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
