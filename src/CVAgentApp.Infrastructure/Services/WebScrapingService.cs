using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CVAgentApp.Infrastructure.Services;

public interface IWebScrapingService
{
    Task<string> ScrapeJobPostingAsync(string jobUrl);
    Task<JobPostingAnalysis> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null);
}

public class JobPostingAnalysis
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    public List<string> PreferredSkills { get; set; } = new();
    public List<string> Responsibilities { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public string JobType { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Benefits { get; set; } = string.Empty;
}

public class WebScrapingService : IWebScrapingService
{
    private readonly IHttpClientService _httpClientService;
    private readonly ILogger<WebScrapingService> _logger;

    public WebScrapingService(IHttpClientService httpClientService, ILogger<WebScrapingService> logger)
    {
        _httpClientService = httpClientService;
        _logger = logger;
    }

    public async Task<string> ScrapeJobPostingAsync(string jobUrl)
    {
        try
        {
            _logger.LogInformation("Scraping job posting from: {JobUrl}", jobUrl);

            // Add headers to mimic a real browser
            var headers = new Dictionary<string, string>
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36" },
                { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8" },
                { "Accept-Language", "en-US,en;q=0.5" },
                { "Accept-Encoding", "gzip, deflate" },
                { "Connection", "keep-alive" },
                { "Upgrade-Insecure-Requests", "1" }
            };

            var response = await _httpClientService.GetAsync(jobUrl, headers);
            var content = await response.Content.ReadAsStringAsync();

            // Clean up the HTML content
            var cleanContent = CleanHtmlContent(content);
            
            _logger.LogInformation("Successfully scraped job posting content");
            return cleanContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping job posting: {JobUrl}", jobUrl);
            throw;
        }
    }

    public async Task<JobPostingAnalysis> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null)
    {
        try
        {
            _logger.LogInformation("Analyzing job posting: {JobUrl}", jobUrl);

            var scrapedContent = await ScrapeJobPostingAsync(jobUrl);
            
            // Extract structured information from the scraped content
            var analysis = new JobPostingAnalysis
            {
                Title = ExtractJobTitle(scrapedContent),
                Company = companyName ?? ExtractCompany(scrapedContent),
                Location = ExtractLocation(scrapedContent),
                SalaryRange = ExtractSalary(scrapedContent),
                RequiredSkills = ExtractRequiredSkills(scrapedContent),
                PreferredSkills = ExtractPreferredSkills(scrapedContent),
                Responsibilities = ExtractResponsibilities(scrapedContent),
                Requirements = ExtractRequirements(scrapedContent),
                JobType = ExtractJobType(scrapedContent),
                ExperienceLevel = ExtractExperienceLevel(scrapedContent),
                Description = ExtractDescription(scrapedContent),
                Benefits = ExtractBenefits(scrapedContent)
            };

            _logger.LogInformation("Job posting analysis completed successfully");
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing job posting: {JobUrl}", jobUrl);
            throw;
        }
    }

    private string CleanHtmlContent(string htmlContent)
    {
        // Remove HTML tags but preserve structure
        var cleanContent = Regex.Replace(htmlContent, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        cleanContent = Regex.Replace(cleanContent, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        cleanContent = Regex.Replace(cleanContent, @"<[^>]+>", " ");
        cleanContent = Regex.Replace(cleanContent, @"\s+", " ");
        return cleanContent.Trim();
    }

    private string ExtractJobTitle(string content)
    {
        // Look for common job title patterns
        var patterns = new[]
        {
            @"Job Title[:\s]+([^\n\r]+)",
            @"Position[:\s]+([^\n\r]+)",
            @"Role[:\s]+([^\n\r]+)",
            @"Title[:\s]+([^\n\r]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return "Software Engineer"; // Default fallback
    }

    private string ExtractCompany(string content)
    {
        var patterns = new[]
        {
            @"Company[:\s]+([^\n\r]+)",
            @"Employer[:\s]+([^\n\r]+)",
            @"Organization[:\s]+([^\n\r]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return "Company Name"; // Default fallback
    }

    private string ExtractLocation(string content)
    {
        var patterns = new[]
        {
            @"Location[:\s]+([^\n\r]+)",
            @"Address[:\s]+([^\n\r]+)",
            @"Where[:\s]+([^\n\r]+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return "Remote/Hybrid"; // Default fallback
    }

    private string ExtractSalary(string content)
    {
        var salaryPattern = @"(?:salary|pay|compensation)[:\s]*([^\n\r]+)";
        var match = Regex.Match(content, salaryPattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : "Competitive";
    }

    private List<string> ExtractRequiredSkills(string content)
    {
        var skills = new List<string>();
        
        // Look for common skill patterns
        var skillPatterns = new[]
        {
            @"(?:required|must have|essential)[:\s]*([^\n\r]+)",
            @"(?:skills|technologies)[:\s]*([^\n\r]+)",
            @"(?:experience with|knowledge of)[:\s]*([^\n\r]+)"
        };

        foreach (var pattern in skillPatterns)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var skillText = match.Groups[1].Value.Trim();
                // Split by common delimiters
                var individualSkills = skillText.Split(new[] { ',', ';', '|', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                skills.AddRange(individualSkills.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        return skills.Distinct().Take(10).ToList();
    }

    private List<string> ExtractPreferredSkills(string content)
    {
        var skills = new List<string>();
        
        var preferredPatterns = new[]
        {
            @"(?:preferred|nice to have|bonus)[:\s]*([^\n\r]+)",
            @"(?:advantage|plus)[:\s]*([^\n\r]+)"
        };

        foreach (var pattern in preferredPatterns)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var skillText = match.Groups[1].Value.Trim();
                var individualSkills = skillText.Split(new[] { ',', ';', '|', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                skills.AddRange(individualSkills.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        return skills.Distinct().Take(5).ToList();
    }

    private List<string> ExtractResponsibilities(string content)
    {
        var responsibilities = new List<string>();
        
        var respPatterns = new[]
        {
            @"(?:responsibilities|duties|what you'll do)[:\s]*([^\n\r]+)",
            @"(?:key tasks|main duties)[:\s]*([^\n\r]+)"
        };

        foreach (var pattern in respPatterns)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var respText = match.Groups[1].Value.Trim();
                var individualResps = respText.Split(new[] { '\n', '•', '-' }, StringSplitOptions.RemoveEmptyEntries);
                responsibilities.AddRange(individualResps.Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)));
            }
        }

        return responsibilities.Take(8).ToList();
    }

    private List<string> ExtractRequirements(string content)
    {
        var requirements = new List<string>();
        
        var reqPatterns = new[]
        {
            @"(?:requirements|qualifications|must have)[:\s]*([^\n\r]+)",
            @"(?:education|experience)[:\s]*([^\n\r]+)"
        };

        foreach (var pattern in reqPatterns)
        {
            var matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var reqText = match.Groups[1].Value.Trim();
                var individualReqs = reqText.Split(new[] { '\n', '•', '-' }, StringSplitOptions.RemoveEmptyEntries);
                requirements.AddRange(individualReqs.Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)));
            }
        }

        return requirements.Take(6).ToList();
    }

    private string ExtractJobType(string content)
    {
        var jobTypePatterns = new[]
        {
            @"(?:full.?time|part.?time|contract|permanent|temporary)",
            @"(?:remote|hybrid|onsite|in.?office)"
        };

        foreach (var pattern in jobTypePatterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Value;
            }
        }

        return "Full-time";
    }

    private string ExtractExperienceLevel(string content)
    {
        var expPatterns = new[]
        {
            @"(?:entry.?level|junior|mid.?level|senior|lead|principal)",
            @"(\d+)\+?\s*years?\s*(?:of\s*)?experience"
        };

        foreach (var pattern in expPatterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Value;
            }
        }

        return "Mid-level";
    }

    private string ExtractDescription(string content)
    {
        // Extract the main job description
        var descPattern = @"(?:description|about|overview)[:\s]*([^\n\r]{50,500})";
        var match = Regex.Match(content, descPattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : "Job description not available";
    }

    private string ExtractBenefits(string content)
    {
        var benefitsPattern = @"(?:benefits|perks|compensation)[:\s]*([^\n\r]+)";
        var match = Regex.Match(content, benefitsPattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : "Competitive benefits package";
    }
}


