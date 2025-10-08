using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CVAgentApp.Infrastructure.Guardrails;

/// <summary>
/// Job posting guardrail to validate job URLs and content
/// </summary>
public class JobPostingGuardrail : IJobPostingGuardrail
{
    private readonly ILogger<JobPostingGuardrail> _logger;

    public string Name => "JobPostingGuardrail";
    public int Priority => 2; // Medium priority

    public JobPostingGuardrail(ILogger<JobPostingGuardrail> logger)
    {
        _logger = logger;
    }

    public async Task<GuardrailResult> ValidateAsync(GuardrailContext context)
    {
        try
        {
            _logger.LogInformation("Executing job posting guardrail for agent {AgentName}", context.AgentName);

            // Validate job URL if present
            if (context.Metadata.ContainsKey("jobUrl"))
            {
                var jobUrl = context.Metadata["jobUrl"].ToString();
                if (!string.IsNullOrEmpty(jobUrl))
                {
                    var urlResult = await ValidateJobUrlAsync(jobUrl);
                    if (urlResult.TripwireTriggered)
                    {
                        return urlResult;
                    }
                }
            }

            // Validate job content if present
            if (!string.IsNullOrEmpty(context.Input))
            {
                var contentResult = await ValidateJobContentAsync(context.Input);
                if (contentResult.TripwireTriggered)
                {
                    return contentResult;
                }
            }

            _logger.LogInformation("Job posting guardrail passed for agent {AgentName}", context.AgentName);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing job posting guardrail");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "GuardrailError",
                Message = "Error in job posting guardrail",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateJobUrlAsync(string jobUrl)
    {
        try
        {
            _logger.LogInformation("Validating job URL: {JobUrl}", jobUrl);

            // Check if URL is well-formed
            if (!Uri.IsWellFormedUriString(jobUrl, UriKind.Absolute))
            {
                _logger.LogWarning("Invalid job URL format: {JobUrl}", jobUrl);
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "InvalidUrlFormat",
                    Message = "Invalid job URL format",
                    AllowExecution = false,
                    Recommendations = new List<string>
                    {
                        "Please provide a valid job posting URL",
                        "Ensure the URL is properly formatted"
                    }
                };
            }

            var uri = new Uri(jobUrl);

            // Check for suspicious domains or patterns
            var suspiciousPatterns = new[]
            {
                @"\b(phishing|scam|fraud|fake)\b",
                @"\b(bit\.ly|tinyurl|short\.link)\b", // Shortened URLs can be suspicious
                @"\b(\.tk|\.ml|\.ga|\.cf)\b" // Suspicious TLDs
            };

            foreach (var pattern in suspiciousPatterns)
            {
                if (Regex.IsMatch(jobUrl, pattern, RegexOptions.IgnoreCase))
                {
                    _logger.LogWarning("Suspicious job URL detected: {JobUrl}", jobUrl);
                    return new GuardrailResult
                    {
                        TripwireTriggered = true,
                        ViolationType = "SuspiciousUrl",
                        Message = "Suspicious job URL detected",
                        AllowExecution = false,
                        Details = new Dictionary<string, object>
                        {
                            ["suspiciousPattern"] = pattern,
                            ["url"] = jobUrl
                        },
                        Recommendations = new List<string>
                        {
                            "Please verify the job posting URL",
                            "Use a direct link to the company's career page",
                            "Avoid shortened or suspicious URLs"
                        }
                    };
                }
            }

            // Check for legitimate job board domains
            var legitimateDomains = new[]
            {
                "linkedin.com", "indeed.com", "glassdoor.com", "monster.com",
                "careerbuilder.com", "ziprecruiter.com", "angel.co", "wellfound.com",
                "dice.com", "simplyhired.com", "jobs.com", "careerjet.com"
            };

            var isLegitimateDomain = legitimateDomains.Any(domain =>
                uri.Host.Contains(domain, StringComparison.OrdinalIgnoreCase));

            if (!isLegitimateDomain)
            {
                _logger.LogWarning("Non-standard job board domain: {Domain}", uri.Host);
                return new GuardrailResult
                {
                    TripwireTriggered = false, // Not a violation, just a warning
                    ViolationType = "NonStandardDomain",
                    Message = "Non-standard job board domain detected",
                    AllowExecution = true,
                    Details = new Dictionary<string, object>
                    {
                        ["domain"] = uri.Host,
                        ["isLegitimate"] = false
                    },
                    Recommendations = new List<string>
                    {
                        "Consider using a well-known job board",
                        "Verify the company's official career page"
                    }
                };
            }

            _logger.LogInformation("Job URL validation passed: {JobUrl}", jobUrl);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating job URL");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating job URL",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateJobContentAsync(string jobContent)
    {
        try
        {
            _logger.LogInformation("Validating job content");

            if (string.IsNullOrWhiteSpace(jobContent))
            {
                _logger.LogWarning("Empty job content provided");
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "EmptyContent",
                    Message = "Job content is empty",
                    AllowExecution = false,
                    Recommendations = new List<string>
                    {
                        "Please provide job posting content",
                        "Ensure the job posting is accessible"
                    }
                };
            }

            // Check for suspicious content patterns
            var suspiciousPatterns = new[]
            {
                @"\b(work from home|remote|earn money|get rich|no experience required)\b",
                @"\b(pyramid|mlm|multi-level marketing|network marketing)\b",
                @"\b(commission only|no salary|unpaid)\b",
                @"\b(urgent|immediate start|apply now|limited time)\b"
            };

            var violations = new List<string>();

            foreach (var pattern in suspiciousPatterns)
            {
                if (Regex.IsMatch(jobContent, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"Suspicious content pattern: {pattern}");
                }
            }

            if (violations.Any())
            {
                _logger.LogWarning("Suspicious job content detected: {Violations}", string.Join("; ", violations));
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "SuspiciousContent",
                    Message = $"Suspicious job content detected: {string.Join("; ", violations)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["violations"] = violations,
                        ["violationCount"] = violations.Count
                    },
                    Recommendations = new List<string>
                    {
                        "Please verify this is a legitimate job posting",
                        "Check the company's official website",
                        "Be cautious of suspicious job postings"
                    }
                };
            }

            // Check for legitimate job posting indicators
            var legitimateIndicators = new[]
            {
                @"\b(requirements|qualifications|responsibilities|benefits)\b",
                @"\b(experience|education|degree|certification)\b",
                @"\b(salary|compensation|pay|benefits)\b",
                @"\b(company|organization|team|department)\b"
            };

            var indicatorCount = legitimateIndicators.Count(pattern =>
                Regex.IsMatch(jobContent, pattern, RegexOptions.IgnoreCase));

            if (indicatorCount < 2)
            {
                _logger.LogWarning("Job content lacks legitimate indicators");
                return new GuardrailResult
                {
                    TripwireTriggered = false, // Not a violation, just a warning
                    ViolationType = "LowQualityContent",
                    Message = "Job content may be incomplete or suspicious",
                    AllowExecution = true,
                    Details = new Dictionary<string, object>
                    {
                        ["indicatorCount"] = indicatorCount,
                        ["contentLength"] = jobContent.Length
                    },
                    Recommendations = new List<string>
                    {
                        "Verify this is a complete job posting",
                        "Check for missing job details",
                        "Ensure the posting is from a legitimate source"
                    }
                };
            }

            _logger.LogInformation("Job content validation passed");
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating job content");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating job content",
                AllowExecution = false
            };
        }
    }
}
