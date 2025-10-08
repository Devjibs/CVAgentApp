using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CVAgentApp.Infrastructure.Agents;

/// <summary>
/// CV Parsing Agent implementation using OpenAI's Responses API
/// </summary>
public class CVParsingAgent : ICVParsingAgent
{
    private readonly ILogger<CVParsingAgent> _logger;
    private readonly IDocumentProcessingService _documentProcessor;
    private readonly IOpenAIService _openAIService;
    private readonly IGuardrailService _guardrailService;

    public CVParsingAgent(
        ILogger<CVParsingAgent> logger,
        IDocumentProcessingService documentProcessor,
        IOpenAIService openAIService,
        IGuardrailService guardrailService)
    {
        _logger = logger;
        _documentProcessor = documentProcessor;
        _openAIService = openAIService;
        _guardrailService = guardrailService;
    }

    public async Task<AgentResult<T>> ExecuteAsync<T>(AgentContext context) where T : class
    {
        try
        {
            _logger.LogInformation("Executing CV Parsing Agent for session {SessionId}", context.SessionId);

            // Validate input with guardrails
            var guardrailContext = new GuardrailContext
            {
                Input = context.Metadata.GetValueOrDefault("cvContent", "").ToString() ?? "",
                AgentName = nameof(CVParsingAgent),
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var guardrailResult = await _guardrailService.ExecuteInputGuardrailsAsync(guardrailContext);
            if (!guardrailResult.AllowExecution)
            {
                return new AgentResult<T>
                {
                    Success = false,
                    ErrorMessage = $"Input guardrail violation: {guardrailResult.Message}"
                };
            }

            // Execute the parsing logic
            var result = await ParseCVAsync(
                context.Metadata.GetValueOrDefault("cvFile") as IFormFile ?? throw new ArgumentException("CV file not found in context"),
                context);

            return new AgentResult<T>
            {
                Success = result.Success,
                Data = result.Data as T,
                ErrorMessage = result.ErrorMessage,
                Metadata = result.Metadata,
                ExecutionTime = TimeSpan.FromMilliseconds(100) // Placeholder
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing CV Parsing Agent");
            return new AgentResult<T>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateInputAsync(AgentContext context)
    {
        if (!context.Metadata.ContainsKey("cvFile"))
        {
            _logger.LogWarning("CV file not found in context");
            return false;
        }

        var cvFile = context.Metadata["cvFile"] as IFormFile;
        if (cvFile == null || cvFile.Length == 0)
        {
            _logger.LogWarning("Invalid CV file in context");
            return false;
        }

        return true;
    }

    public async Task<AgentResult<T>> ExecuteWithGuardrailsAsync<T>(AgentContext context) where T : class
    {
        return await ExecuteAsync<T>(context);
    }

    public async Task<AgentResult<CandidateAnalysisResponse>> ParseCVAsync(IFormFile cvFile, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Parsing CV file: {FileName}", cvFile.FileName);

            // Extract text from CV
            string cvContent;
            using (var stream = cvFile.OpenReadStream())
            {
                cvContent = cvFile.ContentType.ToLower() switch
                {
                    "application/pdf" => await _documentProcessor.ExtractTextFromPDFAsync(stream),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => await _documentProcessor.ExtractTextFromWordAsync(stream),
                    _ => throw new NotSupportedException($"Unsupported file type: {cvFile.ContentType}")
                };
            }

            // Use OpenAI to analyze the CV content
            var analysisPrompt = $$"""
                Analyze the following CV and extract structured information. Return a JSON response with the following structure:
                {{
                    "firstName": "string",
                    "lastName": "string", 
                    "email": "string",
                    "phone": "string",
                    "location": "string",
                    "summary": "string",
                    "workExperiences": [
                        {{
                            "company": "string",
                            "position": "string",
                            "startDate": "YYYY-MM-DD",
                            "endDate": "YYYY-MM-DD or null",
                            "isCurrent": boolean,
                            "description": "string",
                            "achievements": ["string"]
                        }}
                    ],
                    "education": [
                        {{
                            "institution": "string",
                            "degree": "string",
                            "fieldOfStudy": "string",
                            "startDate": "YYYY-MM-DD",
                            "endDate": "YYYY-MM-DD",
                            "gpa": decimal or null
                        }}
                    ],
                    "skills": [
                        {{
                            "name": "string",
                            "level": "Beginner|Intermediate|Advanced|Expert",
                            "category": "Technical|Soft|Language|Industry",
                            "yearsOfExperience": integer
                        }}
                    ],
                    "certifications": [
                        {{
                            "name": "string",
                            "issuingOrganization": "string",
                            "issueDate": "YYYY-MM-DD",
                            "expiryDate": "YYYY-MM-DD or null",
                            "credentialId": "string"
                        }}
                    ],
                    "projects": [
                        {{
                            "name": "string",
                            "description": "string",
                            "startDate": "YYYY-MM-DD",
                            "endDate": "YYYY-MM-DD",
                            "url": "string",
                            "technologies": ["string"]
                        }}
                    ]
                }}

                CV Content:
                {cvContent}
                """;

            var analysisResult = await _openAIService.AnalyzeCandidateAsync(cvContent);

            // Parse the JSON response
            var candidateAnalysis = JsonSerializer.Deserialize<CandidateAnalysisResponse>(analysisResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (candidateAnalysis == null)
            {
                return new AgentResult<CandidateAnalysisResponse>
                {
                    Success = false,
                    ErrorMessage = "Failed to parse candidate analysis"
                };
            }

            // Validate output with guardrails
            var outputGuardrailContext = new GuardrailContext
            {
                Output = analysisResult,
                AgentName = nameof(CVParsingAgent),
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var outputGuardrailResult = await _guardrailService.ExecuteOutputGuardrailsAsync(outputGuardrailContext);
            if (!outputGuardrailResult.AllowExecution)
            {
                return new AgentResult<CandidateAnalysisResponse>
                {
                    Success = false,
                    ErrorMessage = $"Output guardrail violation: {outputGuardrailResult.Message}"
                };
            }

            _logger.LogInformation("Successfully parsed CV for {FirstName} {LastName}", candidateAnalysis.FirstName, candidateAnalysis.LastName);

            return new AgentResult<CandidateAnalysisResponse>
            {
                Success = true,
                Data = candidateAnalysis,
                Metadata = new Dictionary<string, object>
                {
                    ["originalContentLength"] = cvContent.Length,
                    ["extractedSkillsCount"] = candidateAnalysis.Skills.Count,
                    ["workExperienceCount"] = candidateAnalysis.WorkExperiences.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CV");
            return new AgentResult<CandidateAnalysisResponse>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<List<string>>> ExtractSkillsAsync(string cvContent)
    {
        try
        {
            var skillsPrompt = $$"""
                Extract all technical and soft skills from the following CV content. 
                Return a JSON array of skill names.
                
                CV Content:
                {cvContent}
                """;

            var result = await _openAIService.AnalyzeCandidateAsync(cvContent);
            var skills = JsonSerializer.Deserialize<List<string>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<string>();

            return new AgentResult<List<string>>
            {
                Success = true,
                Data = skills
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting skills");
            return new AgentResult<List<string>>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<List<WorkExperienceDto>>> ExtractWorkExperienceAsync(string cvContent)
    {
        try
        {
            var experiencePrompt = $$"""
                Extract work experience from the following CV content.
                Return a JSON array of work experience objects with company, position, dates, and descriptions.
                
                CV Content:
                {cvContent}
                """;

            var result = await _openAIService.AnalyzeCandidateAsync(cvContent);
            var experiences = JsonSerializer.Deserialize<List<WorkExperienceDto>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<WorkExperienceDto>();

            return new AgentResult<List<WorkExperienceDto>>
            {
                Success = true,
                Data = experiences
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting work experience");
            return new AgentResult<List<WorkExperienceDto>>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
