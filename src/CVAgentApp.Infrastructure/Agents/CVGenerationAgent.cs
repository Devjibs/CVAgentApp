using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CVAgentApp.Infrastructure.Agents;

/// <summary>
/// CV Generation Agent implementation for creating tailored CVs and cover letters
/// </summary>
public class CVGenerationAgent : ICVGenerationAgent
{
    private readonly ILogger<CVGenerationAgent> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly IDocumentProcessingService _documentProcessor;
    private readonly IGuardrailService _guardrailService;

    public CVGenerationAgent(
        ILogger<CVGenerationAgent> logger,
        IOpenAIService openAIService,
        IDocumentProcessingService documentProcessor,
        IGuardrailService guardrailService)
    {
        _logger = logger;
        _openAIService = openAIService;
        _documentProcessor = documentProcessor;
        _guardrailService = guardrailService;
    }

    public async Task<AgentResult<T>> ExecuteAsync<T>(AgentContext context) where T : class
    {
        try
        {
            _logger.LogInformation("Executing CV Generation Agent for session {SessionId}", context.SessionId);

            var candidate = context.Metadata.GetValueOrDefault("candidate") as CandidateAnalysisResponse;
            var job = context.Metadata.GetValueOrDefault("job") as JobAnalysisResponse;
            var matching = context.Metadata.GetValueOrDefault("matching") as MatchingResult;

            if (candidate == null || job == null || matching == null)
            {
                return new AgentResult<T>
                {
                    Success = false,
                    ErrorMessage = "Required data not found in context"
                };
            }

            // Execute the generation logic
            var result = await GenerateTailoredCVAsync(candidate, job, matching, context);

            return new AgentResult<T>
            {
                Success = result.Success,
                Data = result.Data as T,
                ErrorMessage = result.ErrorMessage,
                Metadata = result.Metadata,
                ExecutionTime = TimeSpan.FromMilliseconds(300) // Placeholder
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing CV Generation Agent");
            return new AgentResult<T>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateInputAsync(AgentContext context)
    {
        if (!context.Metadata.ContainsKey("candidate") ||
            !context.Metadata.ContainsKey("job") ||
            !context.Metadata.ContainsKey("matching"))
        {
            _logger.LogWarning("Required data not found in context");
            return false;
        }

        return true;
    }

    public async Task<AgentResult<T>> ExecuteWithGuardrailsAsync<T>(AgentContext context) where T : class
    {
        return await ExecuteAsync<T>(context);
    }

    public async Task<AgentResult<string>> GenerateTailoredCVAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job, MatchingResult matching, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Generating tailored CV for {CandidateName} applying to {JobTitle}",
                $"{candidate.FirstName} {candidate.LastName}", job.JobTitle);

            var cvPrompt = $$"""
                Create a tailored CV for the candidate based on the job requirements.
                Focus on relevant skills and experiences that match the job requirements.
                Maintain truthfulness - only include information that exists in the original CV.
                
                Guidelines:
                1. Reorder sections to highlight relevant experience first
                2. Emphasize skills that match job requirements
                3. Use keywords from the job description
                4. Maintain professional formatting
                5. Keep all information truthful and verifiable
                
                Candidate Information:
                {JsonSerializer.Serialize(candidate, new JsonSerializerOptions { WriteIndented = true })}
                
                Job Requirements:
                {JsonSerializer.Serialize(job, new JsonSerializerOptions { WriteIndented = true })}
                
                Matching Analysis:
                Match Score: {matching.MatchScore}
                Matching Skills: {JsonSerializer.Serialize(matching.MatchingSkills)}
                Strengths: {JsonSerializer.Serialize(matching.Strengths)}
                Skill Gaps: {JsonSerializer.Serialize(matching.SkillGaps)}
                
                Generate a professional CV that highlights the candidate's relevant experience and skills for this specific role.
                """;

            var cvContent = await _openAIService.GenerateCVAsync(candidate, job);

            // Validate output with guardrails
            var outputGuardrailContext = new GuardrailContext
            {
                Output = cvContent,
                AgentName = nameof(CVGenerationAgent),
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var outputGuardrailResult = await _guardrailService.ExecuteOutputGuardrailsAsync(outputGuardrailContext);
            if (!outputGuardrailResult.AllowExecution)
            {
                return new AgentResult<string>
                {
                    Success = false,
                    ErrorMessage = $"Output guardrail violation: {outputGuardrailResult.Message}"
                };
            }

            _logger.LogInformation("Successfully generated tailored CV");

            return new AgentResult<string>
            {
                Success = true,
                Data = cvContent,
                Metadata = new Dictionary<string, object>
                {
                    ["contentLength"] = cvContent.Length,
                    ["matchScore"] = matching.MatchScore,
                    ["highlightedSkills"] = matching.MatchingSkills.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tailored CV");
            return new AgentResult<string>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<string>> GenerateCoverLetterAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job, MatchingResult matching, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Generating cover letter for {CandidateName} applying to {JobTitle}",
                $"{candidate.FirstName} {candidate.LastName}", job.JobTitle);

            var coverLetterPrompt = $$"""
                Create a compelling cover letter for the candidate applying to the job.
                The cover letter should:
                1. Reference the company's mission and values
                2. Highlight relevant experience and skills
                3. Demonstrate understanding of the role
                4. Show enthusiasm for the company
                5. Be personalized and professional
                
                Candidate Information:
                {JsonSerializer.Serialize(candidate, new JsonSerializerOptions { WriteIndented = true })}
                
                Job Information:
                {JsonSerializer.Serialize(job, new JsonSerializerOptions { WriteIndented = true })}
                
                Company Information:
                {JsonSerializer.Serialize(job.CompanyInfo, new JsonSerializerOptions { WriteIndented = true })}
                
                Matching Analysis:
                Match Score: {matching.MatchScore}
                Strengths: {JsonSerializer.Serialize(matching.Strengths)}
                
                Generate a professional cover letter that connects the candidate's experience to the company's needs.
                """;

            var coverLetterContent = await _openAIService.GenerateCoverLetterAsync(candidate, job);

            // Validate output with guardrails
            var outputGuardrailContext = new GuardrailContext
            {
                Output = coverLetterContent,
                AgentName = nameof(CVGenerationAgent),
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var outputGuardrailResult = await _guardrailService.ExecuteOutputGuardrailsAsync(outputGuardrailContext);
            if (!outputGuardrailResult.AllowExecution)
            {
                return new AgentResult<string>
                {
                    Success = false,
                    ErrorMessage = $"Output guardrail violation: {outputGuardrailResult.Message}"
                };
            }

            _logger.LogInformation("Successfully generated cover letter");

            return new AgentResult<string>
            {
                Success = true,
                Data = coverLetterContent,
                Metadata = new Dictionary<string, object>
                {
                    ["contentLength"] = coverLetterContent.Length,
                    ["companyReferenced"] = job.CompanyInfo != null,
                    ["personalized"] = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover letter");
            return new AgentResult<string>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<byte[]>> FormatDocumentAsync(string content, DocumentType type, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Formatting document of type {DocumentType}", type);

            byte[] documentBytes;

            switch (type)
            {
                case DocumentType.CV:
                case DocumentType.Resume:
                    documentBytes = await _documentProcessor.GeneratePDFAsync(content);
                    break;
                case DocumentType.CoverLetter:
                    documentBytes = await _documentProcessor.GeneratePDFAsync(content);
                    break;
                default:
                    throw new NotSupportedException($"Document type {type} not supported");
            }

            return new AgentResult<byte[]>
            {
                Success = true,
                Data = documentBytes,
                Metadata = new Dictionary<string, object>
                {
                    ["documentType"] = type.ToString(),
                    ["fileSizeBytes"] = documentBytes.Length
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting document");
            return new AgentResult<byte[]>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
