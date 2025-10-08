using Microsoft.AspNetCore.Http;
using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CVAgentApp.Core.Enums;

namespace CVAgentApp.Infrastructure.Services;

public class CVGenerationService : ICVGenerationService
{
    private readonly ILogger<CVGenerationService> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly IDocumentProcessingService _documentProcessingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISessionService _sessionService;

    public CVGenerationService(
        ILogger<CVGenerationService> logger,
        IOpenAIService openAIService,
        IDocumentProcessingService documentProcessingService,
        IFileStorageService fileStorageService,
        ISessionService sessionService)
    {
        _logger = logger;
        _openAIService = openAIService;
        _documentProcessingService = documentProcessingService;
        _fileStorageService = fileStorageService;
        _sessionService = sessionService;
    }

    public async Task<CVGenerationResponse> GenerateCVAsync(CVGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Starting CV generation process");

            // Step 1: Extract text from uploaded CV
            var cvContent = await ExtractCVContentAsync(request.CVFile);
            _logger.LogInformation("CV content extracted successfully");

            // Step 2: Analyze job posting
            var jobAnalysis = await _openAIService.AnalyzeJobPostingAsync(request.JobPostingUrl, request.CompanyName);
            var jobAnalysisResponse = JsonSerializer.Deserialize<JobAnalysisResponse>(jobAnalysis) ?? new JobAnalysisResponse();
            _logger.LogInformation("Job posting analyzed successfully");

            // Step 3: Analyze candidate CV
            var candidateAnalysis = await _openAIService.AnalyzeCandidateAsync(cvContent);
            var candidateAnalysisResponse = JsonSerializer.Deserialize<CandidateAnalysisResponse>(candidateAnalysis) ?? new CandidateAnalysisResponse();
            _logger.LogInformation("Candidate CV analyzed successfully");

            // Step 4: Generate tailored CV
            var tailoredCV = await _openAIService.GenerateCVAsync(candidateAnalysisResponse, jobAnalysisResponse);
            _logger.LogInformation("Tailored CV generated successfully");

            // Step 5: Generate cover letter
            var coverLetter = await _openAIService.GenerateCoverLetterAsync(candidateAnalysisResponse, jobAnalysisResponse);
            _logger.LogInformation("Cover letter generated successfully");

            // Step 6: Create session
            var candidateId = Guid.NewGuid(); // In real app, this would be from user authentication
            var jobPostingId = Guid.NewGuid(); // In real app, this would be from job posting storage
            var session = await _sessionService.CreateSessionAsync(candidateId, jobPostingId);

            // Step 7: Generate and store documents
            var documents = new List<GeneratedDocumentDto>();

            // Generate CV document
            var cvDocument = await GenerateDocumentAsync(
                tailoredCV,
                "Tailored_CV.pdf",
                DocumentType.CV,
                session.Id);
            documents.Add(cvDocument);

            // Generate cover letter document
            var coverLetterDocument = await GenerateDocumentAsync(
                coverLetter,
                "Cover_Letter.pdf",
                DocumentType.CoverLetter,
                session.Id);
            documents.Add(coverLetterDocument);

            // Step 8: Complete session
            await _sessionService.CompleteSessionAsync(session.Id);

            _logger.LogInformation("CV generation process completed successfully");

            return new CVGenerationResponse
            {
                SessionId = session.Id,
                SessionToken = session.SessionToken,
                Status = CVGenerationStatus.Completed,
                Message = "CV and cover letter generated successfully",
                Documents = documents
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CV generation process");
            return new CVGenerationResponse
            {
                Status = CVGenerationStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SessionStatusResponse> GetSessionStatusAsync(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Getting session status: {SessionToken}", sessionToken);

            var session = await _sessionService.GetSessionAsync(sessionToken);
            if (session == null)
            {
                return new SessionStatusResponse
                {
                    SessionToken = sessionToken,
                    Status = CVGenerationStatus.Failed
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

            return new SessionStatusResponse
            {
                SessionId = session.Id,
                SessionToken = session.SessionToken,
                Status = (CVGenerationStatus)session.Status,
                ProcessingLog = session.ProcessingLog,
                Documents = documents,
                CreatedAt = session.CreatedAt,
                CompletedAt = session.CompletedAt,
                ExpiresAt = session.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status: {SessionToken}", sessionToken);
            throw;
        }
    }

    public async Task<CandidateAnalysisResponse> AnalyzeCandidateAsync(IFormFile cvFile)
    {
        try
        {
            _logger.LogInformation("Analyzing candidate CV");

            var cvContent = await ExtractCVContentAsync(cvFile);
            var analysis = await _openAIService.AnalyzeCandidateAsync(cvContent);
            var result = JsonSerializer.Deserialize<CandidateAnalysisResponse>(analysis) ?? new CandidateAnalysisResponse();

            _logger.LogInformation("Candidate analysis completed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing candidate");
            throw;
        }
    }

    public async Task<JobAnalysisResponse> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null)
    {
        try
        {
            _logger.LogInformation("Analyzing job posting: {JobUrl}", jobUrl);

            var analysis = await _openAIService.AnalyzeJobPostingAsync(jobUrl, companyName);
            var result = JsonSerializer.Deserialize<JobAnalysisResponse>(analysis) ?? new JobAnalysisResponse();

            _logger.LogInformation("Job posting analysis completed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing job posting: {JobUrl}", jobUrl);
            throw;
        }
    }

    public Task<byte[]> DownloadDocumentAsync(Guid documentId)
    {
        try
        {
            _logger.LogInformation("Downloading document: {DocumentId}", documentId);

            // In a real implementation, you would get the document from the database
            // and download it from blob storage
            throw new NotImplementedException("Document download not implemented yet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<bool> DeleteSessionAsync(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Deleting session: {SessionToken}", sessionToken);

            var session = await _sessionService.GetSessionAsync(sessionToken);
            if (session == null)
                return false;

            await _sessionService.ExpireSessionAsync(session.Id);

            _logger.LogInformation("Session deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session: {SessionToken}", sessionToken);
            throw;
        }
    }

    private async Task<string> ExtractCVContentAsync(IFormFile cvFile)
    {
        using var stream = cvFile.OpenReadStream();

        return cvFile.ContentType.ToLower() switch
        {
            "application/pdf" => await _documentProcessingService.ExtractTextFromPDFAsync(stream),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" =>
                await _documentProcessingService.ExtractTextFromWordAsync(stream),
            "application/msword" =>
                await _documentProcessingService.ExtractTextFromWordAsync(stream),
            _ => throw new NotSupportedException($"File type {cvFile.ContentType} is not supported")
        };
    }

    private async Task<GeneratedDocumentDto> GenerateDocumentAsync(
        string content,
        string fileName,
        DocumentType type,
        Guid sessionId)
    {
        try
        {
            _logger.LogInformation("Generating document: {FileName}", fileName);

            // Format the document
            var formattedContent = await _documentProcessingService.FormatDocumentAsync(content, type);

            // Generate PDF
            var pdfBytes = await _documentProcessingService.GeneratePDFAsync(formattedContent);

            // Upload to storage
            using var stream = new MemoryStream(pdfBytes);
            var blobUrl = await _fileStorageService.UploadFileAsync(stream, fileName, "application/pdf");

            // Generate download URL
            var downloadUrl = await _fileStorageService.GenerateDownloadUrlAsync(blobUrl, TimeSpan.FromHours(24));

            _logger.LogInformation("Document generated successfully: {FileName}", fileName);

            return new GeneratedDocumentDto
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Type = type,
                Content = formattedContent,
                DownloadUrl = downloadUrl,
                FileSizeBytes = pdfBytes.Length,
                Status = DocumentStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating document: {FileName}", fileName);
            throw;
        }
    }
}
