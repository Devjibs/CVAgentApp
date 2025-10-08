using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;

namespace CVAgentApp.Core.Interfaces;

/// <summary>
/// Base interface for all agent services in the CV Agent system
/// </summary>
public interface IAgentService
{
    Task<AgentResult<T>> ExecuteAsync<T>(AgentContext context) where T : class;
    Task<bool> ValidateInputAsync(AgentContext context);
    Task<AgentResult<T>> ExecuteWithGuardrailsAsync<T>(AgentContext context) where T : class;
}

/// <summary>
/// CV Parsing Agent - Extracts and structures candidate information from uploaded CV
/// </summary>
public interface ICVParsingAgent : IAgentService
{
    Task<AgentResult<CandidateAnalysisResponse>> ParseCVAsync(IFormFile cvFile, AgentContext context);
    Task<AgentResult<List<string>>> ExtractSkillsAsync(string cvContent);
    Task<AgentResult<List<WorkExperienceDto>>> ExtractWorkExperienceAsync(string cvContent);
}

/// <summary>
/// Job Extraction Agent - Fetches and analyzes job postings from URLs
/// </summary>
public interface IJobExtractionAgent : IAgentService
{
    Task<AgentResult<JobAnalysisResponse>> ExtractJobPostingAsync(string jobUrl, string? companyName, AgentContext context);
    Task<AgentResult<CompanyInfoDto>> ResearchCompanyAsync(string companyName, AgentContext context);
    Task<AgentResult<List<string>>> ExtractRequiredSkillsAsync(string jobDescription);
}

/// <summary>
/// Matching Agent - Compares candidate profile with job requirements
/// </summary>
public interface IMatchingAgent : IAgentService
{
    Task<AgentResult<MatchingResult>> MatchCandidateToJobAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job, AgentContext context);
    Task<AgentResult<List<string>>> IdentifySkillGapsAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job);
    Task<AgentResult<List<string>>> IdentifyStrengthsAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job);
}

/// <summary>
/// CV Generation Agent - Creates tailored CV based on job requirements
/// </summary>
public interface ICVGenerationAgent : IAgentService
{
    Task<AgentResult<string>> GenerateTailoredCVAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job, MatchingResult matching, AgentContext context);
    Task<AgentResult<string>> GenerateCoverLetterAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job, MatchingResult matching, AgentContext context);
    Task<AgentResult<byte[]>> FormatDocumentAsync(string content, DocumentType type, AgentContext context);
}

/// <summary>
/// Review Agent - Validates generated documents for truthfulness and quality
/// </summary>
public interface IReviewAgent : IAgentService
{
    Task<AgentResult<ReviewResult>> ReviewDocumentAsync(string content, DocumentType type, AgentContext context);
    Task<AgentResult<bool>> ValidateTruthfulnessAsync(string originalCV, string generatedCV);
    Task<AgentResult<List<string>>> ExtractFabricatedContentAsync(string originalCV, string generatedCV);
}

/// <summary>
/// Multi-Agent Orchestrator - Coordinates all agents in the workflow
/// </summary>
public interface IMultiAgentOrchestrator
{
    Task<AgentResult<CVGenerationResponse>> ExecuteFullWorkflowAsync(CVGenerationRequest request);
    Task<AgentResult<SessionStatusResponse>> GetWorkflowStatusAsync(string sessionToken);
    Task<AgentResult<bool>> CancelWorkflowAsync(string sessionToken);
}

/// <summary>
/// Context object passed to all agents containing user information and dependencies
/// </summary>
public class AgentContext
{
    public Guid SessionId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> ProcessingLog { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result wrapper for agent operations
/// </summary>
public class AgentResult<T> where T : class
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
}

/// <summary>
/// Result of matching candidate to job
/// </summary>
public class MatchingResult
{
    public double MatchScore { get; set; }
    public List<string> MatchingSkills { get; set; } = new();
    public List<string> SkillGaps { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, double> SkillScores { get; set; } = new();
}

/// <summary>
/// Result of document review
/// </summary>
public class ReviewResult
{
    public bool IsTruthful { get; set; }
    public double QualityScore { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> FabricatedContent { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public bool RequiresHumanReview { get; set; }
}
