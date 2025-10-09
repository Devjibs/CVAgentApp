using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CVAgentApp.Core.Enums;

namespace CVAgentApp.Infrastructure.Agents;

/// <summary>
/// Review Agent implementation for validating generated documents
/// </summary>
public class ReviewAgent : IReviewAgent
{
    private readonly ILogger<ReviewAgent> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly IGuardrailService _guardrailService;

    public ReviewAgent(
        ILogger<ReviewAgent> logger,
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
            _logger.LogInformation("Executing Review Agent for session {SessionId}", context.SessionId);

            var content = context.Metadata.GetValueOrDefault("content", "").ToString() ?? "";
            var documentType = context.Metadata.GetValueOrDefault("documentType", DocumentType.CV) as DocumentType? ?? DocumentType.CV;

            // Execute the review logic
            var result = await ReviewDocumentAsync(content, documentType, context);

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
            _logger.LogError(ex, "Error executing Review Agent");
            return new AgentResult<T>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateInputAsync(AgentContext context)
    {
        if (!context.Metadata.ContainsKey("content"))
        {
            _logger.LogWarning("Content not found in context");
            return false;
        }

        var content = context.Metadata["content"].ToString();
        if (string.IsNullOrEmpty(content))
        {
            _logger.LogWarning("Empty content in context");
            return false;
        }

        return true;
    }

    public async Task<AgentResult<T>> ExecuteWithGuardrailsAsync<T>(AgentContext context) where T : class
    {
        return await ExecuteAsync<T>(context);
    }

    public async Task<AgentResult<ReviewResult>> ReviewDocumentAsync(string content, DocumentType type, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Reviewing document of type {DocumentType}", type);

            // Create review prompt
            var reviewPrompt = $@"
                Analyze the match between the candidate and job requirements.
                Calculate a match score (0-100) and provide detailed analysis.
                Return a JSON response with the following structure:
                {{
                    ""isTruthful"": boolean,
                    ""qualityScore"": number (0-100),
                    ""issues"": [""string""],
                    ""fabricatedContent"": [""string""],
                    ""recommendations"": [""string""],
                    ""requiresHumanReview"": boolean
                }}
           
                Document Type: {type}
                Document Content:
                {content}

                Check for:
                1. Truthfulness - no fabricated information
                2. Quality - professional formatting and content
                3. Compliance - no discriminatory content
                4. Completeness - all required sections present
                5. ATS compatibility - proper formatting for applicant tracking systems
                ";

            var reviewResult = await _openAIService.AnalyzeCandidateAsync(reviewPrompt);

            // Parse the JSON response
            var review = JsonSerializer.Deserialize<ReviewResult>(reviewResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (review == null)
            {
                return new AgentResult<ReviewResult>
                {
                    Success = false,
                    ErrorMessage = "Failed to parse review result"
                };
            }

            // Validate output with guardrails
            var outputGuardrailContext = new GuardrailContext
            {
                Output = reviewResult,
                AgentName = nameof(ReviewAgent),
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var outputGuardrailResult = await _guardrailService.ExecuteOutputGuardrailsAsync(outputGuardrailContext);
            if (!outputGuardrailResult.AllowExecution)
            {
                return new AgentResult<ReviewResult>
                {
                    Success = false,
                    ErrorMessage = $"Output guardrail violation: {outputGuardrailResult.Message}"
                };
            }

            _logger.LogInformation("Document review completed with quality score: {QualityScore}", review.QualityScore);

            return new AgentResult<ReviewResult>
            {
                Success = true,
                Data = review,
                Metadata = new Dictionary<string, object>
                {
                    ["qualityScore"] = review.QualityScore,
                    ["isTruthful"] = review.IsTruthful,
                    ["issuesCount"] = review.Issues.Count,
                    ["requiresHumanReview"] = review.RequiresHumanReview
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing document");
            return new AgentResult<ReviewResult>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<ValidationResult>> ValidateTruthfulnessAsync(string originalCV, string generatedCV)
    {
        try
        {
            _logger.LogInformation("Validating truthfulness of generated CV");

            var truthfulnessPrompt = $"""
                Compare the original CV with the generated CV to ensure no information was fabricated.
                Return true if the generated CV only contains information that exists in the original CV.
                Return false if any new information was added that doesn't exist in the original.
                
                Original CV:
                {originalCV}
                
                Generated CV:
                {generatedCV}
                
                Return only "true" or "false".
                """;

            var result = await _openAIService.AnalyzeCandidateAsync(truthfulnessPrompt);
            var isTruthful = result.Trim().ToLower() == "true";

            return new AgentResult<ValidationResult>
            {
                Success = true,
                Data = new ValidationResult
                {
                    IsValid = isTruthful,
                    Issues = isTruthful ? new List<string>() : new List<string> { "Generated CV contains fabricated content" },
                    Recommendations = isTruthful ? new List<string>() : new List<string> { "Review and remove fabricated content" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating truthfulness");
            return new AgentResult<ValidationResult>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<List<string>>> ExtractFabricatedContentAsync(string originalCV, string generatedCV)
    {
        try
        {
            _logger.LogInformation("Extracting fabricated content from generated CV");

            var fabricationPrompt = $"""
                Identify any content in the generated CV that does not exist in the original CV.
                Return a JSON array of fabricated content items.
                
                Original CV:
                {originalCV}
                
                Generated CV:
                {generatedCV}
                
                Return a JSON array of strings containing fabricated content.
                """;

            var result = await _openAIService.AnalyzeCandidateAsync(fabricationPrompt);
            var fabricatedContent = JsonSerializer.Deserialize<List<string>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<string>();

            return new AgentResult<List<string>>
            {
                Success = true,
                Data = fabricatedContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting fabricated content");
            return new AgentResult<List<string>>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}



