using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CVAgentApp.Infrastructure.Agents;

/// <summary>
/// Job Extraction Agent implementation using OpenAI's Responses API with web search
/// </summary>
public class JobExtractionAgent : IJobExtractionAgent
{
    private readonly ILogger<JobExtractionAgent> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly IGuardrailService _guardrailService;
    private readonly HttpClient _httpClient;

    public JobExtractionAgent(
        ILogger<JobExtractionAgent> logger,
        IOpenAIService openAIService,
        IGuardrailService guardrailService,
        HttpClient httpClient)
    {
        _logger = logger;
        _openAIService = openAIService;
        _guardrailService = guardrailService;
        _httpClient = httpClient;
    }

    public async Task<AgentResult<T>> ExecuteAsync<T>(AgentContext context) where T : class
    {
        try
        {
            _logger.LogInformation("Executing Job Extraction Agent for session {SessionId}", context.SessionId);

            var jobUrl = context.Metadata.GetValueOrDefault("jobUrl", "").ToString() ?? "";
            var companyName = context.Metadata.GetValueOrDefault("companyName", "").ToString();

            // Validate input with guardrails
            var guardrailContext = new GuardrailContext
            {
                Input = jobUrl,
                AgentName = nameof(JobExtractionAgent),
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

            // Execute the job extraction logic
            var result = await ExtractJobPostingAsync(jobUrl, companyName, context);

            return new AgentResult<T>
            {
                Success = result.Success,
                Data = result.Data as T,
                ErrorMessage = result.ErrorMessage,
                Metadata = result.Metadata,
                ExecutionTime = TimeSpan.FromMilliseconds(200) // Placeholder
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Job Extraction Agent");
            return new AgentResult<T>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ValidateInputAsync(AgentContext context)
    {
        if (!context.Metadata.ContainsKey("jobUrl"))
        {
            _logger.LogWarning("Job URL not found in context");
            return false;
        }

        var jobUrl = context.Metadata["jobUrl"].ToString();
        if (string.IsNullOrEmpty(jobUrl) || !Uri.IsWellFormedUriString(jobUrl, UriKind.Absolute))
        {
            _logger.LogWarning("Invalid job URL in context");
            return false;
        }

        return true;
    }

    public async Task<AgentResult<T>> ExecuteWithGuardrailsAsync<T>(AgentContext context) where T : class
    {
        return await ExecuteAsync<T>(context);
    }

    public async Task<AgentResult<JobAnalysisResponse>> ExtractJobPostingAsync(string jobUrl, string? companyName, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Extracting job posting from URL: {JobUrl}", jobUrl);

            // Fetch job posting content
            var jobContent = await FetchJobPostingContentAsync(jobUrl);
            if (string.IsNullOrEmpty(jobContent))
            {
                return new AgentResult<JobAnalysisResponse>
                {
                    Success = false,
                    ErrorMessage = "Failed to fetch job posting content"
                };
            }

            // Use OpenAI with web search to analyze the job posting
            var analysisPrompt = $@"
                Analyze the following job posting and extract structured information. Use web search to gather additional company information if needed.
                Return a JSON response with the following structure:
                {{
                    ""jobTitle"": ""string"",
                    ""company"": ""string"",
                    ""location"": ""string"",
                    ""employmentType"": ""FullTime|PartTime|Contract|Internship|Temporary"",
                    ""experienceLevel"": ""Entry|Mid|Senior|Lead|Executive"",
                    ""description"": ""string"",
                    ""requirements"": ""string"",
                    ""responsibilities"": ""string"",
                    ""requiredSkills"": [""string""],
                    ""requiredQualifications"": [""string""],
                    ""companyInfo"": {{
                        ""name"": ""string"",
                        ""mission"": ""string"",
                        ""description"": ""string"",
                        ""industry"": ""string"",
                        ""size"": ""string"",
                        ""website"": ""string"",
                        ""values"": [""string""],
                        ""recentNews"": [""string""]
                    }}
                }}

                Job Posting Content:
                {jobContent}

                Company Name (if provided): {companyName ?? "Not provided"}
                ";

            var analysisResult = await _openAIService.AnalyzeJobPostingAsync(jobUrl, companyName);

            // Parse the JSON response
            var jobAnalysis = JsonSerializer.Deserialize<JobAnalysisResponse>(analysisResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (jobAnalysis == null)
            {
                return new AgentResult<JobAnalysisResponse>
                {
                    Success = false,
                    ErrorMessage = "Failed to parse job analysis"
                };
            }

            // Validate output with guardrails
            var outputGuardrailContext = new GuardrailContext
            {
                Output = analysisResult,
                AgentName = nameof(JobExtractionAgent),
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var outputGuardrailResult = await _guardrailService.ExecuteOutputGuardrailsAsync(outputGuardrailContext);
            if (!outputGuardrailResult.AllowExecution)
            {
                return new AgentResult<JobAnalysisResponse>
                {
                    Success = false,
                    ErrorMessage = $"Output guardrail violation: {outputGuardrailResult.Message}"
                };
            }

            _logger.LogInformation("Successfully extracted job posting for {JobTitle} at {Company}", jobAnalysis.JobTitle, jobAnalysis.Company);

            return new AgentResult<JobAnalysisResponse>
            {
                Success = true,
                Data = jobAnalysis,
                Metadata = new Dictionary<string, object>
                {
                    ["originalContentLength"] = jobContent.Length,
                    ["requiredSkillsCount"] = jobAnalysis.RequiredSkills.Count,
                    ["companyInfoAvailable"] = jobAnalysis.CompanyInfo != null
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting job posting");
            return new AgentResult<JobAnalysisResponse>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<CompanyInfoDto>> ResearchCompanyAsync(string companyName, AgentContext context)
    {
        try
        {
            _logger.LogInformation("Researching company: {CompanyName}", companyName);

            var researchPrompt = $@"
                Research the following company and provide detailed information.
                Use web search to gather current information about the company.
                Return a JSON response with the following structure:
                {{
                    ""name"": ""string"",
                    ""mission"": ""string"",
                    ""description"": ""string"",
                    ""industry"": ""string"",
                    ""size"": ""string"",
                    ""website"": ""string"",
                    ""values"": [""string""],
                    ""recentNews"": [""string""]
                }}

                Company Name: {companyName}
                ";

            var researchResult = await _openAIService.ResearchCompanyAsync(companyName);

            var companyInfo = JsonSerializer.Deserialize<CompanyInfoDto>(researchResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (companyInfo == null)
            {
                return new AgentResult<CompanyInfoDto>
                {
                    Success = false,
                    ErrorMessage = "Failed to parse company research"
                };
            }

            return new AgentResult<CompanyInfoDto>
            {
                Success = true,
                Data = companyInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error researching company");
            return new AgentResult<CompanyInfoDto>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<AgentResult<List<string>>> ExtractRequiredSkillsAsync(string jobDescription)
    {
        try
        {
            var skillsPrompt = $@"
                Extract all required skills and technologies from the following job description.
                Return a JSON array of skill names.
                
                Job Description:
                {jobDescription}
                ";

            var result = await _openAIService.AnalyzeJobPostingAsync("", null);
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
            _logger.LogError(ex, "Error extracting required skills");
            return new AgentResult<List<string>>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<string> FetchJobPostingContentAsync(string jobUrl)
    {
        try
        {
            _logger.LogInformation("Fetching job posting content from: {JobUrl}", jobUrl);

            var response = await _httpClient.GetAsync(jobUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch job posting: {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            var content = await response.Content.ReadAsStringAsync();

            // Basic HTML cleaning - in a real implementation, you'd use a proper HTML parser
            var cleanContent = Regex.Replace(content, @"<[^>]+>", " ");
            cleanContent = Regex.Replace(cleanContent, @"\s+", " ");

            return cleanContent.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching job posting content");
            return string.Empty;
        }
    }
}



