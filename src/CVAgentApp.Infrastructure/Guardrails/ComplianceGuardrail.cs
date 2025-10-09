using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CVAgentApp.Infrastructure.Guardrails;

/// <summary>
/// Compliance guardrail to validate discrimination compliance and protected attributes
/// </summary>
public class ComplianceGuardrail : IComplianceGuardrail
{
    private readonly ILogger<ComplianceGuardrail> _logger;

    public string Name => "ComplianceGuardrail";
    public int Priority => 1; // High priority

    public ComplianceGuardrail(ILogger<ComplianceGuardrail> logger)
    {
        _logger = logger;
    }

    public async Task<GuardrailResult> ValidateAsync(GuardrailContext context)
    {
        try
        {
            _logger.LogInformation("Executing compliance guardrail for agent {AgentName}", context.AgentName);

            if (string.IsNullOrEmpty(context.Output))
            {
                _logger.LogWarning("Empty output provided to compliance guardrail");
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "EmptyOutput",
                    Message = "No output content to validate",
                    AllowExecution = false
                };
            }

            // Validate discrimination compliance
            var discriminationResult = await ValidateDiscriminationComplianceAsync(context.Output);
            if (discriminationResult.TripwireTriggered)
            {
                return discriminationResult;
            }

            // Validate protected attributes
            var protectedAttributesResult = await ValidateProtectedAttributesAsync(context.Output);
            if (protectedAttributesResult.TripwireTriggered)
            {
                return protectedAttributesResult;
            }

            _logger.LogInformation("Compliance guardrail passed for agent {AgentName}", context.AgentName);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing compliance guardrail");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "GuardrailError",
                Message = "Error in compliance guardrail",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateDiscriminationComplianceAsync(string content)
    {
        try
        {
            _logger.LogInformation("Validating discrimination compliance");

            var violations = new List<string>();

            // Check for discriminatory language patterns
            var discriminatoryPatterns = new[]
            {
                // Age discrimination
                @"\b(young|old|elderly|senior citizen|millennial|gen z|baby boomer)\b",
                @"\b(fresh graduate|recent graduate|new to the field|entry level)\b",
                
                // Gender discrimination
                @"\b(male|female|man|woman|guy|girl|ladies|gentlemen)\b",
                @"\b(masculine|feminine|macho|girly|tomboy|sissy)\b",
                
                // Race/ethnicity discrimination
                @"\b(white|black|asian|hispanic|latino|native american|indian|chinese|japanese|korean)\b",
                @"\b(african american|caucasian|european|middle eastern|arab|muslim|christian|jewish)\b",
                
                // Disability discrimination
                @"\b(disabled|handicapped|retarded|mentally ill|physically challenged)\b",
                @"\b(blind|deaf|deaf-mute|wheelchair bound|paralyzed)\b",
                
                // Religion discrimination
                @"\b(christian|muslim|jewish|hindu|buddhist|atheist|agnostic)\b",
                @"\b(church|mosque|temple|synagogue|religious|spiritual)\b",
                
                // Sexual orientation discrimination
                @"\b(gay|lesbian|bisexual|transgender|straight|heterosexual|homosexual)\b",
                @"\b(queer|fag|dyke|tranny|sissy|butch|femme)\b",
                
                // National origin discrimination
                @"\b(foreign|immigrant|alien|refugee|visa|citizenship|nationality)\b",
                @"\b(american|british|canadian|australian|german|french|spanish|italian)\b"
            };

            foreach (var pattern in discriminatoryPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"Potentially discriminatory language: {pattern}");
                }
            }

            // Check for implicit bias indicators
            var biasPatterns = new[]
            {
                @"\b(aggressive|assertive|dominant|submissive|emotional|logical)\b",
                @"\b(ambitious|competitive|nurturing|caring|technical|creative)\b",
                @"\b(leadership|management|team player|individual contributor)\b"
            };

            var biasCount = 0;
            foreach (var pattern in biasPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    biasCount++;
                }
            }

            if (biasCount > 3)
            {
                violations.Add("Potential implicit bias in language");
            }

            if (violations.Any())
            {
                _logger.LogWarning("Discrimination compliance violations detected: {Violations}", string.Join("; ", violations));
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "DiscriminationViolation",
                    Message = $"Discrimination compliance violations detected: {string.Join("; ", violations)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["violations"] = violations,
                        ["violationCount"] = violations.Count,
                        ["biasCount"] = biasCount
                    },
                    Recommendations = new List<string>
                    {
                        "Remove any references to protected characteristics",
                        "Use gender-neutral language",
                        "Focus on skills and qualifications only",
                        "Avoid language that could imply bias",
                        "Ensure equal opportunity language"
                    }
                };
            }

            _logger.LogInformation("Discrimination compliance validation passed");
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating discrimination compliance");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating discrimination compliance",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateProtectedAttributesAsync(string content)
    {
        try
        {
            _logger.LogInformation("Validating protected attributes");

            var violations = new List<string>();

            // Check for protected attributes that should not be included
            var protectedAttributePatterns = new[]
            {
                // Age-related
                @"\b\d{1,2}\s*(years?\s*old|years?\s*of\s*age)\b",
                @"\b(born\s*in|birth\s*year|age\s*of)\b",
                
                // Gender-related
                @"\b(mr\.|mrs\.|ms\.|miss|sir|madam|gentleman|lady)\b",
                @"\b(he|she|him|her|his|hers|himself|herself)\b",
                
                // Marital status
                @"\b(single|married|divorced|widowed|engaged|separated)\b",
                @"\b(spouse|husband|wife|partner|significant other)\b",
                
                // Family status
                @"\b(children|kids|family|parent|mother|father|pregnant)\b",
                @"\b(expecting|due\s*date|maternity|paternity)\b",
                
                // Health/disability
                @"\b(disabled|handicapped|ill|sick|healthy|medical|health)\b",
                @"\b(medication|treatment|therapy|rehabilitation)\b",
                
                // Religion
                @"\b(church|mosque|temple|synagogue|religious|spiritual)\b",
                @"\b(sunday|sabbath|holy|sacred|faith|belief)\b",
                
                // Political affiliation
                @"\b(republican|democrat|liberal|conservative|political)\b",
                @"\b(vote|election|campaign|party|government)\b",
                
                // National origin
                @"\b(immigrant|foreign|alien|visa|citizenship|nationality)\b",
                @"\b(country\s*of\s*origin|place\s*of\s*birth|native\s*country)\b"
            };

            foreach (var pattern in protectedAttributePatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"Protected attribute detected: {pattern}");
                }
            }

            // Check for personal information that should not be in professional documents
            var personalInfoPatterns = new[]
            {
                @"\b(ssn|social\s*security|tax\s*id|driver\s*license)\b",
                @"\b(passport|visa|immigration|citizenship)\b",
                @"\b(credit\s*score|credit\s*history|financial|debt)\b",
                @"\b(criminal|arrest|conviction|felony|misdemeanor)\b"
            };

            foreach (var pattern in personalInfoPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"Personal information detected: {pattern}");
                }
            }

            if (violations.Any())
            {
                _logger.LogWarning("Protected attributes violations detected: {Violations}", string.Join("; ", violations));
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "ProtectedAttributesViolation",
                    Message = $"Protected attributes violations detected: {string.Join("; ", violations)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["violations"] = violations,
                        ["violationCount"] = violations.Count
                    },
                    Recommendations = new List<string>
                    {
                        "Remove all references to protected characteristics",
                        "Focus only on professional qualifications",
                        "Avoid personal information in professional documents",
                        "Ensure compliance with equal opportunity laws",
                        "Use neutral, professional language only"
                    }
                };
            }

            _logger.LogInformation("Protected attributes validation passed");
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating protected attributes");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating protected attributes",
                AllowExecution = false
            };
        }
    }
}



