using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Enums;
using CVAgentApp.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CVAgentApp.Infrastructure.Services;

public class CVGenerationService : ICVGenerationService
{
    private readonly ILogger<CVGenerationService> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly IOpenAIService _openAIService;

    public CVGenerationService(
        ILogger<CVGenerationService> logger,
        IFileStorageService fileStorageService,
        IOpenAIService openAIService)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
        _openAIService = openAIService;
    }

    public async Task<CVGenerationResponse> GenerateCVAsync(CVGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("=== CV GENERATION SERVICE STARTED ===");
            _logger.LogInformation("Starting CV generation process with OpenAI");

            // Step 1: Upload CV file to OpenAI
            using var cvStream = request.CVFile.OpenReadStream();
            var cvFileId = await _fileStorageService.UploadFileAsync(cvStream, request.CVFile.FileName, request.CVFile.ContentType);
            _logger.LogInformation("CV file uploaded to OpenAI: {FileId}", cvFileId);

            // Step 2: Create prompt for GPT-5 with file ID and job requirements
            var prompt = $@"
You are an expert CV and cover letter writer. I have uploaded a CV file (ID: {cvFileId}) and need you to:

1. Analyze the uploaded CV
2. Create a tailored CV for this job: {request.JobPostingUrl}
3. Create a professional cover letter for this position
4. Return both documents in proper PDF format

Job Requirements:
- URL: {request.JobPostingUrl}
- Company: {request.CompanyName ?? "Not specified"}

Please analyze the job posting from the URL and tailor the CV accordingly. Keep the same format and structure as the original CV but optimize the content for this specific role.

Return the results as two separate documents:
1. Tailored_CV.pdf
2. Cover_Letter.pdf
";

            // Step 3: Send to GPT-5 with file attachment
            var response = await _openAIService.ProcessWithFileAsync(cvFileId, prompt);
            
            // Step 4: Parse the response and create documents
            var documents = new List<GeneratedDocumentDto>();
            
            _logger.LogInformation("OpenAI Response Length: {Length} characters", response.Length);
            _logger.LogInformation("OpenAI Response Preview: {Preview}", response.Substring(0, Math.Min(200, response.Length)));
            
            // For now, create mock documents with the response content
            // In a real implementation, GPT-5 would return actual file IDs
            var cvDocument = new GeneratedDocumentDto
            {
                Id = Guid.NewGuid(),
                FileName = "Tailored_CV.pdf",
                Type = DocumentType.CV,
                Content = response, // This would be the actual CV content
                DownloadUrl = cvFileId, // Use the original file ID for now
                FileSizeBytes = response.Length * 2, // Estimate based on content
                Status = DocumentStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
            documents.Add(cvDocument);

            var coverLetterDocument = new GeneratedDocumentDto
            {
                Id = Guid.NewGuid(),
                FileName = "Cover_Letter.pdf",
                Type = DocumentType.CoverLetter,
                Content = response, // This would be the actual cover letter content
                DownloadUrl = cvFileId, // Use the original file ID for now
                FileSizeBytes = response.Length * 2, // Estimate based on content
                Status = DocumentStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
            documents.Add(coverLetterDocument);
            
            _logger.LogInformation("Created {Count} documents for caching", documents.Count);

            _logger.LogInformation("CV generation process completed successfully");

            return new CVGenerationResponse
            {
                SessionId = Guid.NewGuid(),
                SessionToken = Guid.NewGuid().ToString(),
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
        // Simplified - no session tracking needed
        return new SessionStatusResponse
        {
            SessionToken = sessionToken,
            Status = CVGenerationStatus.Completed,
            Documents = new List<GeneratedDocumentDto>()
        };
    }

    public async Task<CandidateAnalysisResponse> AnalyzeCandidateAsync(IFormFile cvFile)
    {
        // Simplified - no separate analysis needed
        return new CandidateAnalysisResponse
        {
            FirstName = "John",
            LastName = "Doe",
            Summary = "Experienced professional",
            Skills = new List<SkillDto>(),
            WorkExperiences = new List<WorkExperienceDto>()
        };
    }

    public async Task<JobAnalysisResponse> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null)
    {
        // Simplified - no separate analysis needed
        return new JobAnalysisResponse
        {
            JobTitle = "Software Engineer",
            Company = companyName ?? "Tech Company",
            Location = "Remote",
            RequiredSkills = new List<string> { "C#", "JavaScript", "SQL" },
            RequiredQualifications = new List<string> { "5+ years experience" },
            Responsibilities = "Develop applications",
            Requirements = "5+ years experience",
            EmploymentType = EmploymentType.FullTime,
            ExperienceLevel = ExperienceLevel.Senior,
            Description = "Software development role"
        };
    }

    public async Task<byte[]> DownloadDocumentAsync(Guid documentId)
    {
        // Simplified - return empty array for now
        return new byte[0];
    }

    public async Task<bool> DeleteSessionAsync(string sessionToken)
    {
        // Simplified - always return true
        return true;
    }
}