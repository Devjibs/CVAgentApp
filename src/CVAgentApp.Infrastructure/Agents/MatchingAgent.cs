using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CVAgentApp.Infrastructure.Agents;

/// <summary>
/// Matching Agent implementation for comparing candidate profile with job requirements
/// </summary>
public class MatchingAgent : IMatchingAgent
{
    private readonly ILogger<MatchingAgent> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly IGuardrailService _guardrailService;

    public MatchingAgent(
        ILogger<MatchingAgent> logger,
        IOpenAIService openAIService,
        IGuardrailService guardrailService)
    {
        _logger = logger;
        _openAIService = openAIService;
        _guardrailService = guardrailService;
    }

    public async Task<AgentResult<T>> ExecuteAsync<T>(AgentContext context) where T : class
    {
        try
        {
            _logger.LogInformation("Executing Matching Agent for session {SessionId}", context.SessionId);

            var candidate = context.Metadata.GetValueOrDefault("candidate") as CandidateAnalysisResponse;
            var job = context.Metadata.GetValueOrDefault("job") as JobAnalysisResponse;

            if (candidate == null || job == null)
            {
                return new AgentResult<T>
                {
                    Success = false,
                    ErrorMessage = "Candidate or job data not found in context"
                };
            }

            // Execute the matching logic
            var result = await MatchCandidateToJobAsync(candidate, job, context);

            return new AgentResult<T>
            {
                Success = result.Success,
                Data = result.Data as T,
                ErrorMessage = result.ErrorMessage,
                Metadata = result.Metadata,
                ExecutionTime = TimeSpan.FromMilliseconds(150) // Placeholder
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Matching Agent");
            return new AgentResult<T>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateInputAsync(AgentContext context)
    {
        if (!context.Metadata.ContainsKey("candidate") || !context.Metadata.ContainsKey("job"))
        {
            _logger.LogWarning("Candidate or job data not found in context");
            return false;
        }

        return true;
    }

    public async Task<AgentResult<T>> ExecuteWithGuardrailsAsync<T>(AgentContext context) where T : class
    {
        return await ExecuteAsync<T>(context);
    }

    public async Task<AgentResult<MatchingResult>> MatchCandidateToJobAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Matching candidate {CandidateName} to job {JobTitle}",
                $"{candidate.FirstName} {candidate.LastName}", job.JobTitle);

            // Create matching prompt
            var matchingPrompt = $@"
                Analyze the match between the candidate and job requirements.
                Calculate a match score (0-100) and provide detailed analysis.
                Return a JSON response with the following structure:
                {{
                    ""matchScore"": number (0-100),
                    ""matchingSkills"": [""string""],
                    ""skillGaps"": [""string""],
                    ""strengths"": [""string""],
                    ""recommendations"": [""string""],
                    ""skillScores"": {{
                        ""skillName"": number (0-100)
                    }}
                }}

                Candidate Information:
                Name: {candidate.FirstName} {candidate.LastName}
                Skills: {JsonSerializer.Serialize(candidate.Skills.Select(s => s.Name))}
                Work Experience: {JsonSerializer.Serialize(candidate.WorkExperiences.Select(w => w.Position))}
                Education: {JsonSerializer.Serialize(candidate.Education.Select(e => e.Degree))}

                Job Requirements:
                Title: {job.JobTitle}
                Company: {job.Company}
                Required Skills: {JsonSerializer.Serialize(job.RequiredSkills)}
                Required Qualifications: {JsonSerializer.Serialize(job.RequiredQualifications)}
                Description: {job.Description}
                ";

            var matchingResult = await _openAIService.AnalyzeCandidateAsync(matchingPrompt);

            // Parse the JSON response
            var matchAnalysis = JsonSerializer.Deserialize<MatchingResult>(matchingResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (matchAnalysis == null)
            {
                return new AgentResult<MatchingResult>
                {
                    Success = false,
                    ErrorMessage = "Failed to parse matching analysis"
                };
            }

            // Validate output with guardrails
            var outputGuardrailContext = new GuardrailContext
            {
                Output = matchingResult,
                AgentName = nameof(MatchingAgent),
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var outputGuardrailResult = await _guardrailService.ExecuteOutputGuardrailsAsync(outputGuardrailContext);
            if (!outputGuardrailResult.AllowExecution)
            {
                return new AgentResult<MatchingResult>
                {
                    Success = false,
                    ErrorMessage = $"Output guardrail violation: {outputGuardrailResult.Message}"
                };
            }

            _logger.LogInformation("Match analysis completed with score: {MatchScore}", matchAnalysis.MatchScore);

            return new AgentResult<MatchingResult>
            {
                Success = true,
                Data = matchAnalysis,
                Metadata = new Dictionary<string, object>
                {
                    ["matchScore"] = matchAnalysis.MatchScore,
                    ["matchingSkillsCount"] = matchAnalysis.MatchingSkills.Count,
                    ["skillGapsCount"] = matchAnalysis.SkillGaps.Count,
                    ["strengthsCount"] = matchAnalysis.Strengths.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error matching candidate to job");
            return new AgentResult<MatchingResult>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<List<string>>> IdentifySkillGapsAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
    {
        try
        {
            var candidateSkills = candidate.Skills.Select(s => s.Name.ToLower()).ToHashSet();
            var requiredSkills = job.RequiredSkills.Select(s => s.ToLower()).ToHashSet();

            var skillGaps = requiredSkills.Except(candidateSkills).ToList();

            return new AgentResult<List<string>>
            {
                Success = true,
                Data = skillGaps
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying skill gaps");
            return new AgentResult<List<string>>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<List<string>>> IdentifyStrengthsAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
    {
        try
        {
            var candidateSkills = candidate.Skills.Select(s => s.Name.ToLower()).ToHashSet();
            var requiredSkills = job.RequiredSkills.Select(s => s.ToLower()).ToHashSet();

            var strengths = candidateSkills.Intersect(requiredSkills).ToList();

            return new AgentResult<List<string>>
            {
                Success = true,
                Data = strengths
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying strengths");
            return new AgentResult<List<string>>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
