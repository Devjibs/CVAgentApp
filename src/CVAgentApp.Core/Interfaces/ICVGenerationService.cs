using Microsoft.AspNetCore.Http;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using CVAgentApp.Core.Enums;

namespace CVAgentApp.Core.Interfaces;

public interface ICVGenerationService
{
    Task<CVGenerationResponse> GenerateCVAsync(CVGenerationRequest request);
    Task<SessionStatusResponse> GetSessionStatusAsync(string sessionToken);
    Task<CandidateAnalysisResponse> AnalyzeCandidateAsync(IFormFile cvFile);
    Task<JobAnalysisResponse> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null);
    Task<byte[]> DownloadDocumentAsync(Guid documentId);
    Task<bool> DeleteSessionAsync(string sessionToken);
}

public interface IOpenAIService
{
    Task<string> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null);
    Task<string> AnalyzeCandidateAsync(string cvContent);
    Task<string> GenerateCVAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job);
    Task<string> GenerateCoverLetterAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job);
    Task<string> ResearchCompanyAsync(string companyName);
}

public interface IDocumentProcessingService
{
    Task<string> ExtractTextFromPDFAsync(Stream pdfStream);
    Task<string> ExtractTextFromWordAsync(Stream wordStream);
    Task<byte[]> GeneratePDFAsync(string content);
    Task<byte[]> GenerateWordAsync(string content);
    Task<string> FormatDocumentAsync(string content, DocumentType type);
}

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string blobUrl);
    Task<bool> DeleteFileAsync(string blobUrl);
    Task<string> GenerateDownloadUrlAsync(string blobUrl, TimeSpan expiry);
}

public interface ISessionService
{
    Task<Session> CreateSessionAsync(Guid candidateId, Guid jobPostingId);
    Task<Session?> GetSessionAsync(string sessionToken);
    Task<bool> UpdateSessionStatusAsync(Guid sessionId, SessionStatus status, string? log = null);
    Task<bool> CompleteSessionAsync(Guid sessionId);
    Task<bool> ExpireSessionAsync(Guid sessionId);
    Task<bool> CleanupExpiredSessionsAsync();
}
