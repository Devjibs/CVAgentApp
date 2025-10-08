using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CVAgentApp.Infrastructure.Guardrails;

/// <summary>
/// Truthfulness guardrail to prevent hallucination in generated content
/// </summary>
public class TruthfulnessGuardrail : ITruthfulnessGuardrail
{
    private readonly ILogger<TruthfulnessGuardrail> _logger;

    public string Name => "TruthfulnessGuardrail";
    public int Priority => 1; // High priority

    public TruthfulnessGuardrail(ILogger<TruthfulnessGuardrail> logger)
    {
        _logger = logger;
    }

    public async Task<GuardrailResult> ValidateAsync(GuardrailContext context)
    {
        try
        {
            _logger.LogInformation("Executing truthfulness guardrail for agent {AgentName}", context.AgentName);

            // Check if we have both original and generated content for comparison
            if (context.Metadata.ContainsKey("originalContent") && context.Metadata.ContainsKey("generatedContent"))
            {
                var originalContent = context.Metadata["originalContent"].ToString() ?? "";
                var generatedContent = context.Metadata["generatedContent"].ToString() ?? "";

                return await ValidateTruthfulnessAsync(originalContent, generatedContent);
            }

            // If no original content available, perform basic truthfulness checks
            return await PerformBasicTruthfulnessChecks(context.Output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing truthfulness guardrail");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "GuardrailError",
                Message = "Error in truthfulness guardrail",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateTruthfulnessAsync(string originalContent, string generatedContent)
    {
        try
        {
            _logger.LogInformation("Validating truthfulness of generated content");

            var fabricatedContent = await DetectFabricatedSkillsAsync(originalContent, generatedContent);
            if (fabricatedContent.Any())
            {
                _logger.LogWarning("Fabricated content detected: {FabricatedContent}", string.Join(", ", fabricatedContent));

                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "FabricatedContent",
                    Message = $"Fabricated content detected: {string.Join(", ", fabricatedContent)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["fabricatedContent"] = fabricatedContent,
                        ["fabricatedCount"] = fabricatedContent.Count
                    },
                    Recommendations = new List<string>
                    {
                        "Remove fabricated content from generated document",
                        "Ensure all information exists in original CV",
                        "Review generated content for accuracy"
                    }
                };
            }

            _logger.LogInformation("Truthfulness validation passed");
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating truthfulness");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating truthfulness",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> DetectFabricatedSkillsAsync(string originalCV, string generatedCV)
    {
        try
        {
            _logger.LogInformation("Detecting fabricated skills in generated CV");

            // Extract skills from both CVs using simple pattern matching
            var originalSkills = ExtractSkills(originalCV);
            var generatedSkills = ExtractSkills(generatedCV);

            // Find skills in generated CV that don't exist in original
            var fabricatedSkills = generatedSkills.Except(originalSkills, StringComparer.OrdinalIgnoreCase).ToList();

            if (fabricatedSkills.Any())
            {
                _logger.LogWarning("Fabricated skills detected: {FabricatedSkills}", string.Join(", ", fabricatedSkills));

                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "FabricatedSkills",
                    Message = $"Fabricated skills detected: {string.Join(", ", fabricatedSkills)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["fabricatedSkills"] = fabricatedSkills,
                        ["fabricatedCount"] = fabricatedSkills.Count
                    }
                };
            }

            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting fabricated skills");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "DetectionError",
                Message = "Error detecting fabricated skills",
                AllowExecution = false
            };
        }
    }

    private async Task<GuardrailResult> PerformBasicTruthfulnessChecks(string content)
    {
        try
        {
            // Check for common fabrication patterns
            var fabricationPatterns = new[]
            {
                @"\b(never mentioned|not in original|fabricated|made up)\b",
                @"\b(completely new|entirely fabricated|invented)\b"
            };

            var violations = new List<string>();

            foreach (var pattern in fabricationPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"Potential fabrication pattern detected: {pattern}");
                }
            }

            if (violations.Any())
            {
                _logger.LogWarning("Basic truthfulness checks failed: {Violations}", string.Join("; ", violations));

                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "FabricationPattern",
                    Message = $"Potential fabrication detected: {string.Join("; ", violations)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["violations"] = violations,
                        ["violationCount"] = violations.Count
                    }
                };
            }

            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing basic truthfulness checks");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "CheckError",
                Message = "Error performing truthfulness checks",
                AllowExecution = false
            };
        }
    }

    private List<string> ExtractSkills(string content)
    {
        // Simple skill extraction using common patterns
        var skillPatterns = new[]
        {
            @"\b(?:Proficient in|Experienced with|Skilled in|Expert in)\s+([A-Za-z\s,]+?)(?:\s|$|,|\.)",
            @"\b([A-Za-z]+(?:\s+[A-Za-z]+)*)\s+(?:programming|development|design|management|analysis)\b",
            @"\b(?:C#|JavaScript|Python|Java|SQL|React|Angular|Vue|Node\.js|ASP\.NET|Entity Framework|Azure|AWS|Docker|Kubernetes|Git|Agile|Scrum)\b"
        };

        var skills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pattern in skillPatterns)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var skill = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(skill) && skill.Length > 1)
                {
                    skills.Add(skill);
                }
            }
        }

        return skills.ToList();
    }
}
