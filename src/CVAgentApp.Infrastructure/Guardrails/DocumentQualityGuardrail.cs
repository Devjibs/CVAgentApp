using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using CVAgentApp.Core.Enums;
using System.Linq;

namespace CVAgentApp.Infrastructure.Guardrails;

/// <summary>
/// Document quality guardrail to validate generated document quality and ATS compatibility
/// </summary>
public class DocumentQualityGuardrail : IDocumentQualityGuardrail
{
    private readonly ILogger<DocumentQualityGuardrail> _logger;

    public string Name => "DocumentQualityGuardrail";
    public int Priority => 3; // Medium priority

    public DocumentQualityGuardrail(ILogger<DocumentQualityGuardrail> logger)
    {
        _logger = logger;
    }

    public async Task<GuardrailResult> ValidateAsync(GuardrailContext context)
    {
        try
        {
            _logger.LogInformation("Executing document quality guardrail for agent {AgentName}", context.AgentName);

            if (string.IsNullOrEmpty(context.Output))
            {
                _logger.LogWarning("Empty output provided to document quality guardrail");
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "EmptyOutput",
                    Message = "No output content to validate",
                    AllowExecution = false
                };
            }

            // Get document type from context
            var documentType = DocumentType.CV;
            if (context.Metadata.ContainsKey("documentType"))
            {
                documentType = (DocumentType)context.Metadata["documentType"];
            }

            // Validate document quality
            var qualityResult = await ValidateDocumentQualityAsync(context.Output, documentType);
            if (qualityResult.TripwireTriggered)
            {
                return qualityResult;
            }

            // Validate ATS compatibility
            var atsResult = await ValidateATSCompatibilityAsync(context.Output);
            if (atsResult.TripwireTriggered)
            {
                return atsResult;
            }

            _logger.LogInformation("Document quality guardrail passed for agent {AgentName}", context.AgentName);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing document quality guardrail");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "GuardrailError",
                Message = "Error in document quality guardrail",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateDocumentQualityAsync(string content, DocumentType type)
    {
        try
        {
            _logger.LogInformation("Validating document quality for type: {DocumentType}", type);

            var issues = new List<string>();

            // Check content length
            if (content.Length < 200)
            {
                issues.Add("Document is too short");
            }
            else if (content.Length > 10000)
            {
                issues.Add("Document is too long");
            }

            // Check for required sections based on document type
            var requiredSections = GetRequiredSections(type);
            foreach (var section in requiredSections)
            {
                if (!content.ToLower().Contains(section.ToLower()))
                {
                    issues.Add($"Missing required section: {section}");
                }
            }

            // Check for formatting issues
            var formattingIssues = CheckFormattingIssues(content);
            issues.AddRange(formattingIssues);

            // Check for professional language
            var languageIssues = CheckProfessionalLanguage(content);
            issues.AddRange(languageIssues);

            if (issues.Any())
            {
                _logger.LogWarning("Document quality issues detected: {Issues}", string.Join("; ", issues));
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "QualityIssues",
                    Message = $"Document quality issues detected: {string.Join("; ", issues)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["issues"] = issues,
                        ["issueCount"] = issues.Count,
                        ["contentLength"] = content.Length,
                        ["documentType"] = type.ToString()
                    },
                    Recommendations = new List<string>
                    {
                        "Ensure all required sections are present",
                        "Check formatting and structure",
                        "Use professional language throughout",
                        "Verify content completeness"
                    }
                };
            }

            _logger.LogInformation("Document quality validation passed");
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document quality");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating document quality",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateATSCompatibilityAsync(string content)
    {
        try
        {
            _logger.LogInformation("Validating ATS compatibility");

            var issues = new List<string>();

            // Check for ATS-unfriendly elements
            var atsUnfriendlyPatterns = new[]
            {
                @"\b(tables|columns|text boxes|images|graphics)\b",
                @"\b(creative fonts|decorative text|artistic elements)\b",
                @"\b(complex formatting|special characters|symbols)\b"
            };

            foreach (var pattern in atsUnfriendlyPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    issues.Add($"ATS-unfriendly element detected: {pattern}");
                }
            }

            // Check for proper keyword density
            var keywordDensity = CalculateKeywordDensity(content);
            if (keywordDensity < 0.02) // Less than 2% keywords
            {
                issues.Add("Low keyword density - may not pass ATS screening");
            }

            // Check for proper section headers
            var sectionHeaders = ExtractSectionHeaders(content);
            if (sectionHeaders.Count < 3)
            {
                issues.Add("Insufficient section headers for ATS parsing");
            }

            // Check for contact information format
            var contactInfoIssues = CheckContactInformationFormat(content);
            issues.AddRange(contactInfoIssues);

            if (issues.Any())
            {
                _logger.LogWarning("ATS compatibility issues detected: {Issues}", string.Join("; ", issues));
                return new GuardrailResult
                {
                    TripwireTriggered = false, // Not a blocking issue, just a warning
                    ViolationType = "ATSCompatibilityIssues",
                    Message = $"ATS compatibility issues detected: {string.Join("; ", issues)}",
                    AllowExecution = true,
                    Details = new Dictionary<string, object>
                    {
                        ["issues"] = issues,
                        ["issueCount"] = issues.Count,
                        ["keywordDensity"] = keywordDensity,
                        ["sectionHeaders"] = sectionHeaders.Count
                    },
                    Recommendations = new List<string>
                    {
                        "Use simple, clean formatting",
                        "Include relevant keywords from job description",
                        "Ensure proper section headers",
                        "Format contact information clearly",
                        "Avoid complex layouts and graphics"
                    }
                };
            }

            _logger.LogInformation("ATS compatibility validation passed");
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ATS compatibility");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating ATS compatibility",
                AllowExecution = false
            };
        }
    }

    private List<string> GetRequiredSections(DocumentType type)
    {
        return type switch
        {
            DocumentType.CV => new List<string> { "Contact", "Experience", "Education", "Skills" },
            DocumentType.CoverLetter => new List<string> { "Introduction", "Body", "Conclusion" },
            DocumentType.Portfolio => new List<string> { "Contact", "Summary", "Experience", "Education" },
            _ => new List<string>()
        };
    }

    private List<string> CheckFormattingIssues(string content)
    {
        var issues = new List<string>();

        // Check for excessive whitespace
        if (Regex.IsMatch(content, @"\s{5,}"))
        {
            issues.Add("Excessive whitespace detected");
        }

        // Check for inconsistent formatting
        var lineLengths = content.Split('\n').Select(line => line.Length).ToList();
        var avgLineLength = lineLengths.Average();
        var maxLineLength = lineLengths.Max();

        if (maxLineLength > avgLineLength * 3)
        {
            issues.Add("Inconsistent line lengths detected");
        }

        // Check for missing punctuation
        var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var sentencesWithoutPunctuation = sentences.Count(s => !s.Trim().EndsWith('.') && !s.Trim().EndsWith('!') && !s.Trim().EndsWith('?'));

        if (sentencesWithoutPunctuation > sentences.Length * 0.1) // More than 10% of sentences
        {
            issues.Add("Missing punctuation in sentences");
        }

        return issues;
    }

    private List<string> CheckProfessionalLanguage(string content)
    {
        var issues = new List<string>();

        // Check for unprofessional language
        var unprofessionalPatterns = new[]
        {
            @"\b(awesome|cool|amazing|fantastic|incredible)\b",
            @"\b(like|um|uh|you know|basically)\b",
            @"\b(hopefully|maybe|perhaps|probably)\b"
        };

        foreach (var pattern in unprofessionalPatterns)
        {
            if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
            {
                issues.Add($"Unprofessional language detected: {pattern}");
            }
        }

        // Check for proper capitalization
        var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var uncapitalizedSentences = sentences.Count(s => !char.IsUpper(s.Trim()[0]));

        if (uncapitalizedSentences > sentences.Length * 0.1)
        {
            issues.Add("Improper capitalization in sentences");
        }

        return issues;
    }

    private double CalculateKeywordDensity(string content)
    {
        var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var totalWords = words.Length;

        if (totalWords == 0) return 0;

        // Common professional keywords
        var keywords = new[]
        {
            "experience", "skills", "education", "certification", "project", "management",
            "development", "analysis", "leadership", "communication", "teamwork", "problem",
            "solution", "achievement", "responsibility", "collaboration", "innovation"
        };

        var keywordCount = words.Count(word => keywords.Contains(word.ToLower()));
        return (double)keywordCount / totalWords;
    }

    private List<string> ExtractSectionHeaders(string content)
    {
        var headers = new List<string>();
        var lines = content.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.Length > 0 && trimmedLine.Length < 50 &&
                char.IsUpper(trimmedLine[0]) &&
                !trimmedLine.Contains('.') &&
                !trimmedLine.Contains(','))
            {
                headers.Add(trimmedLine);
            }
        }

        return headers;
    }

    private List<string> CheckContactInformationFormat(string content)
    {
        var issues = new List<string>();

        // Check for email format
        if (!Regex.IsMatch(content, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"))
        {
            issues.Add("No valid email address found");
        }

        // Check for phone number format
        if (!Regex.IsMatch(content, @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b"))
        {
            issues.Add("No valid phone number found");
        }

        return issues;
    }
}
