using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace CVAgentApp.Infrastructure.Guardrails;

/// <summary>
/// CV Content guardrail to validate CV files and content
/// </summary>
public class CVContentGuardrail : ICVContentGuardrail
{
    private readonly ILogger<CVContentGuardrail> _logger;

    public string Name => "CVContentGuardrail";
    public int Priority => 2; // Medium priority

    public CVContentGuardrail(ILogger<CVContentGuardrail> logger)
    {
        _logger = logger;
    }

    public async Task<GuardrailResult> ValidateAsync(GuardrailContext context)
    {
        try
        {
            _logger.LogInformation("Executing CV content guardrail for agent {AgentName}", context.AgentName);

            // Validate CV file if present
            if (context.Metadata.ContainsKey("cvFile"))
            {
                var cvFile = context.Metadata["cvFile"] as IFormFile;
                if (cvFile != null)
                {
                    var fileResult = await ValidateCVFileAsync(cvFile);
                    if (fileResult.TripwireTriggered)
                    {
                        return fileResult;
                    }
                }
            }

            // Validate CV content if present
            if (!string.IsNullOrEmpty(context.Input))
            {
                var contentResult = await ValidateCVContentAsync(context.Input);
                if (contentResult.TripwireTriggered)
                {
                    return contentResult;
                }
            }

            _logger.LogInformation("CV content guardrail passed for agent {AgentName}", context.AgentName);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing CV content guardrail");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "GuardrailError",
                Message = "Error in CV content guardrail",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateCVFileAsync(IFormFile cvFile)
    {
        try
        {
            _logger.LogInformation("Validating CV file: {FileName}", cvFile.FileName);

            // Check file size (max 10MB)
            if (cvFile.Length > 10 * 1024 * 1024)
            {
                _logger.LogWarning("CV file too large: {FileSize} bytes", cvFile.Length);
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "FileTooLarge",
                    Message = "CV file size must be less than 10MB",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["fileSize"] = cvFile.Length,
                        ["maxSize"] = 10 * 1024 * 1024
                    },
                    Recommendations = new List<string>
                    {
                        "Please compress the CV file",
                        "Remove unnecessary images or formatting",
                        "Try uploading a smaller file"
                    }
                };
            }

            // Check file type
            var allowedTypes = new[]
            {
                "application/pdf",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/msword"
            };

            if (!allowedTypes.Contains(cvFile.ContentType.ToLower()))
            {
                _logger.LogWarning("Invalid CV file type: {ContentType}", cvFile.ContentType);
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "InvalidFileType",
                    Message = "Only PDF and Word documents are supported",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["contentType"] = cvFile.ContentType,
                        ["allowedTypes"] = allowedTypes
                    },
                    Recommendations = new List<string>
                    {
                        "Please convert your CV to PDF or Word format",
                        "Ensure the file is not corrupted",
                        "Try uploading a different file"
                    }
                };
            }

            // Check file name for suspicious patterns
            var suspiciousPatterns = new[]
            {
                @"\b(\.exe|\.bat|\.cmd|\.scr|\.pif|\.com)\b",
                @"\b(script|executable|program)\b"
            };

            foreach (var pattern in suspiciousPatterns)
            {
                if (Regex.IsMatch(cvFile.FileName, pattern, RegexOptions.IgnoreCase))
                {
                    _logger.LogWarning("Suspicious CV file name: {FileName}", cvFile.FileName);
                    return new GuardrailResult
                    {
                        TripwireTriggered = true,
                        ViolationType = "SuspiciousFileName",
                        Message = "Suspicious file name detected",
                        AllowExecution = false,
                        Details = new Dictionary<string, object>
                        {
                            ["fileName"] = cvFile.FileName,
                            ["suspiciousPattern"] = pattern
                        },
                        Recommendations = new List<string>
                        {
                            "Please use a standard CV file name",
                            "Avoid executable file extensions",
                            "Ensure the file is a legitimate CV document"
                        }
                    };
                }
            }

            _logger.LogInformation("CV file validation passed: {FileName}", cvFile.FileName);
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CV file");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating CV file",
                AllowExecution = false
            };
        }
    }

    public async Task<GuardrailResult> ValidateCVContentAsync(string cvContent)
    {
        try
        {
            _logger.LogInformation("Validating CV content");

            if (string.IsNullOrWhiteSpace(cvContent))
            {
                _logger.LogWarning("Empty CV content provided");
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "EmptyContent",
                    Message = "CV content is empty",
                    AllowExecution = false,
                    Recommendations = new List<string>
                    {
                        "Please provide a valid CV document",
                        "Ensure the CV contains readable text",
                        "Try uploading a different CV file"
                    }
                };
            }

            // Check for minimum content length
            if (cvContent.Length < 100)
            {
                _logger.LogWarning("CV content too short: {Length} characters", cvContent.Length);
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "ContentTooShort",
                    Message = "CV content appears to be too short",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["contentLength"] = cvContent.Length,
                        ["minLength"] = 100
                    },
                    Recommendations = new List<string>
                    {
                        "Please provide a more detailed CV",
                        "Ensure all sections are properly filled",
                        "Add more information about your experience"
                    }
                };
            }

            // Check for suspicious content patterns
            var suspiciousPatterns = new[]
            {
                @"\b(password|login|username|secret|confidential)\b",
                @"\b(credit card|bank account|social security|ssn)\b",
                @"\b(bitcoin|cryptocurrency|wallet|trading)\b",
                @"\b(phishing|scam|fraud|illegal)\b"
            };

            var violations = new List<string>();

            foreach (var pattern in suspiciousPatterns)
            {
                if (Regex.IsMatch(cvContent, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"Suspicious content pattern: {pattern}");
                }
            }

            if (violations.Any())
            {
                _logger.LogWarning("Suspicious CV content detected: {Violations}", string.Join("; ", violations));
                return new GuardrailResult
                {
                    TripwireTriggered = true,
                    ViolationType = "SuspiciousContent",
                    Message = $"Suspicious content detected: {string.Join("; ", violations)}",
                    AllowExecution = false,
                    Details = new Dictionary<string, object>
                    {
                        ["violations"] = violations,
                        ["violationCount"] = violations.Count
                    },
                    Recommendations = new List<string>
                    {
                        "Please remove sensitive information from your CV",
                        "Ensure the CV contains only professional information",
                        "Avoid including personal financial details"
                    }
                };
            }

            // Check for legitimate CV indicators
            var legitimateIndicators = new[]
            {
                @"\b(experience|work|employment|job)\b",
                @"\b(education|degree|university|college)\b",
                @"\b(skills|abilities|competencies)\b",
                @"\b(contact|email|phone|address)\b"
            };

            var indicatorCount = legitimateIndicators.Count(pattern =>
                Regex.IsMatch(cvContent, pattern, RegexOptions.IgnoreCase));

            if (indicatorCount < 2)
            {
                _logger.LogWarning("CV content lacks legitimate indicators");
                return new GuardrailResult
                {
                    TripwireTriggered = false, // Not a violation, just a warning
                    ViolationType = "LowQualityContent",
                    Message = "CV content may be incomplete or not properly formatted",
                    AllowExecution = true,
                    Details = new Dictionary<string, object>
                    {
                        ["indicatorCount"] = indicatorCount,
                        ["contentLength"] = cvContent.Length
                    },
                    Recommendations = new List<string>
                    {
                        "Ensure your CV includes work experience, education, and skills",
                        "Add contact information",
                        "Use professional formatting"
                    }
                };
            }

            _logger.LogInformation("CV content validation passed");
            return new GuardrailResult
            {
                TripwireTriggered = false,
                AllowExecution = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CV content");
            return new GuardrailResult
            {
                TripwireTriggered = true,
                ViolationType = "ValidationError",
                Message = "Error validating CV content",
                AllowExecution = false
            };
        }
    }
}
