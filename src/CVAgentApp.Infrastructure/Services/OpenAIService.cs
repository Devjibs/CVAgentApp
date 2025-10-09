using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using CVAgentApp.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CVAgentApp.Infrastructure.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ILogger<OpenAIService> _logger;
    private readonly IHttpClientService _httpClientService;
    private readonly string _apiKey;

    public OpenAIService(ILogger<OpenAIService> logger, IConfiguration configuration, IHttpClientService httpClientService)
    {
        _logger = logger;
        _httpClientService = httpClientService;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
    }

    public async Task<string> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null)
    {
        try
        {
            _logger.LogInformation("Analyzing job posting: {JobUrl}", jobUrl);

            var prompt = $@"
Analyze this job posting and extract the following information in JSON format:

Job URL: {jobUrl}
Company: {companyName ?? "Not specified"}

Please provide:
1. Job title
2. Company name
3. Location
4. Employment type (FullTime, PartTime, Contract, Internship, Temporary)
5. Experience level (Entry, Mid, Senior, Lead, Executive)
6. Job description
7. Requirements
8. Responsibilities
9. Required skills (with proficiency levels)
10. Required qualifications
11. Company information (mission, description, industry, size, website, values, recent news)

Format the response as a structured JSON object.
";

            try
            {
                // Use web scraping to get actual job posting content
                var webScrapingService = new WebScrapingService(_httpClientService, _logger as ILogger<WebScrapingService>);
                var jobAnalysis = await webScrapingService.AnalyzeJobPostingAsync(jobUrl, companyName);
                
                // Create structured analysis
                var analysis = new
                {
                    jobTitle = jobAnalysis.Title,
                    company = jobAnalysis.Company,
                    location = jobAnalysis.Location,
                    salaryRange = jobAnalysis.SalaryRange,
                    requiredSkills = jobAnalysis.RequiredSkills,
                    preferredSkills = jobAnalysis.PreferredSkills,
                    responsibilities = jobAnalysis.Responsibilities,
                    requirements = jobAnalysis.Requirements,
                    jobType = jobAnalysis.JobType,
                    experienceLevel = jobAnalysis.ExperienceLevel,
                    description = jobAnalysis.Description,
                    benefits = jobAnalysis.Benefits
                };
                
                return await Task.FromResult(System.Text.Json.JsonSerializer.Serialize(analysis, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing job posting: {JobUrl}", jobUrl);
                // Fallback to mock data if scraping fails
                var mockResponse = @"{
                    ""jobTitle"": ""Senior Software Engineer"",
                    ""company"": ""TechCorp"",
                    ""requiredSkills"": [""C#"", ""JavaScript"", ""SQL"", ""React"", ""Node.js""],
                    ""preferredSkills"": [""Azure"", ""Docker"", ""Kubernetes""],
                    ""experienceLevel"": ""Senior"",
                    ""location"": ""San Francisco, CA"",
                    ""salaryRange"": ""$120,000 - $150,000"",
                    ""jobType"": ""Full-time"",
                    ""remoteWork"": ""Hybrid"",
                    ""benefits"": [""Health Insurance"", ""401k"", ""Flexible Hours""],
                    ""companySize"": ""500-1000 employees"",
                    ""industry"": ""Technology"",
                    ""mission"": ""Building innovative software solutions"",
                    ""values"": [""Innovation"", ""Collaboration"", ""Excellence""]
                }";
                return await Task.FromResult(mockResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing job posting: {JobUrl}", jobUrl);
            throw;
        }
    }

    public Task<string> AnalyzeCandidateAsync(string cvContent)
    {
        try
        {
            _logger.LogInformation("Analyzing candidate CV");

            var prompt = $@"
Analyze this CV and extract the following information in JSON format:

CV Content:
{cvContent}

Please provide:
1. Personal information (firstName, lastName, email, phone, location)
2. Professional summary
3. Work experience (company, position, startDate, endDate, isCurrent, description, achievements)
4. Education (institution, degree, fieldOfStudy, startDate, endDate, gpa)
5. Skills (name, level, category, yearsOfExperience)
6. Certifications (name, issuingOrganization, issueDate, expiryDate, credentialId)
7. Projects (name, description, startDate, endDate, url, technologies)

Format the response as a structured JSON object.
";

            // TODO: Implement actual OpenAI API call
            // Temporary mock response for testing
            var mockResponse = @"{
                ""name"": ""John Doe"",
                ""email"": ""john.doe@email.com"",
                ""phone"": ""+1-555-0123"",
                ""location"": ""San Francisco, CA"",
                ""summary"": ""Experienced software engineer with 5+ years of experience in full-stack development"",
                ""skills"": [""C#"", ""JavaScript"", ""SQL"", ""React"", ""Node.js"", ""Azure""],
                ""experience"": [
                    {
                        ""title"": ""Senior Software Engineer"",
                        ""company"": ""Tech Corp"",
                        ""duration"": ""2020 - Present"",
                        ""description"": ""Led development of scalable web applications serving 100K+ users""
                    }
                ],
                ""education"": [
                    {
                        ""degree"": ""Bachelor of Science in Computer Science"",
                        ""school"": ""University of Technology"",
                        ""year"": ""2019""
                    }
                ],
                ""certifications"": [""AWS Certified Developer"", ""Microsoft Azure Developer Associate""]
            }";
            return Task.FromResult(mockResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing candidate CV");
            throw;
        }
    }

    public async Task<string> GenerateCVAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
    {
        try
        {
            _logger.LogInformation("Generating tailored CV");

            var prompt = $@"
Generate a tailored CV for this candidate based on the job requirements:

CANDIDATE PROFILE:
- Name: {candidate.FirstName} {candidate.LastName}
- Summary: {candidate.Summary}
- Skills: {string.Join(", ", candidate.Skills.Select(s => s.Name))}
- Experience: {candidate.WorkExperiences.Count} positions

JOB REQUIREMENTS:
- Title: {job.JobTitle}
- Company: {job.Company}
- Requirements: {job.Requirements}
- Required Skills: {string.Join(", ", job.RequiredSkills)}

Please generate a professional CV that:
1. Highlights relevant experience for this specific role
2. Emphasizes skills that match the job requirements
3. Uses keywords from the job posting
4. Maintains professional formatting
5. Includes a tailored professional summary

Format as a well-structured CV document.
";

            // TODO: Implement actual OpenAI API call
            // For now, return a mock response
            var mockCV = $@"{candidate.FirstName} {candidate.LastName}
            {candidate.Summary}
            
            Skills: {string.Join(", ", candidate.Skills.Select(s => s.Name))}
            
            Tailored for: {job.JobTitle} at {job.Company}";
            
            return await Task.FromResult(mockCV);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CV");
            throw;
        }
    }

    public async Task<string> GenerateCoverLetterAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
    {
        try
        {
            _logger.LogInformation("Generating cover letter");

            var prompt = $@"
Generate a tailored cover letter for this candidate and job:

CANDIDATE PROFILE:
- Name: {candidate.FirstName} {candidate.LastName}
- Summary: {candidate.Summary}
- Key Skills: {string.Join(", ", candidate.Skills.Take(5).Select(s => s.Name))}

JOB DETAILS:
- Title: {job.JobTitle}
- Company: {job.Company}
- Description: {job.Description}
- Requirements: {job.Requirements}

Please generate a professional cover letter that:
1. Addresses the hiring manager
2. Explains why the candidate is interested in this specific role
3. Highlights relevant experience and skills
4. Shows knowledge of the company
5. Demonstrates enthusiasm and professionalism
6. Is 3-4 paragraphs long

Format as a professional cover letter.
";

            // TODO: Implement actual OpenAI API call
            // For now, return a mock response
            var mockCoverLetter = $@"Dear Hiring Manager,

I am writing to express my strong interest in the {job.JobTitle} position at {job.Company}. With my background in {string.Join(", ", candidate.Skills.Take(3).Select(s => s.Name))}, I am excited about the opportunity to contribute to your team.

My experience aligns well with your requirements. I have relevant skills and experience that make me a strong candidate for this role.

Key highlights of my background:
• {candidate.Summary}
• Relevant experience and achievements
• Understanding of the role and company needs

I am particularly drawn to this opportunity and believe my expertise would make me a valuable addition to your team.

I would welcome the opportunity to discuss how my skills and experience can contribute to your continued success. Thank you for considering my application.

Best regards,
{candidate.FirstName} {candidate.LastName}";
            
            return await Task.FromResult(mockCoverLetter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover letter");
            throw;
        }
    }

    public Task<string> ResearchCompanyAsync(string companyName)
    {
        try
        {
            _logger.LogInformation("Researching company: {CompanyName}", companyName);

            var prompt = $@"
Research this company and provide information in JSON format:

Company: {companyName}

Please provide:
1. Company name
2. Mission statement
3. Company description
4. Industry
5. Company size
6. Website
7. Core values
8. Recent news or developments

Format the response as a structured JSON object.
";

            // TODO: Implement actual OpenAI API call
            throw new NotImplementedException("OpenAI company research not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error researching company: {CompanyName}", companyName);
            throw;
        }
    }

    public async Task<string> ProcessWithFileAsync(string fileId, string prompt)
    {
        try
        {
            _logger.LogInformation("Processing file with OpenAI: {FileId}", fileId);

            // TODO: Implement actual OpenAI API call with file attachment
            // This would use the OpenAI Assistants API or Chat Completions API with file attachment
            // For now, return a mock response
            
            var mockResponse = $@"
TAILORED CV

John Doe
Senior Software Engineer
Email: john.doe@email.com | Phone: +1-555-0123 | Location: San Francisco, CA

PROFESSIONAL SUMMARY
Experienced software engineer with 5+ years of experience in full-stack development, specializing in C# and JavaScript. Proven track record of leading development teams and delivering high-performance web applications. Strong background in cloud technologies including Azure and AWS.

TECHNICAL SKILLS
• Programming Languages: C#, JavaScript, SQL, Python
• Frameworks: .NET Core, React, Node.js, Angular
• Databases: SQL Server, MongoDB, PostgreSQL
• Cloud Platforms: AWS, Azure
• Tools: Git, Docker, Jenkins, Kubernetes

PROFESSIONAL EXPERIENCE

Senior Software Engineer | Tech Corp | 2020 - Present
• Led development of scalable web applications serving 100K+ users
• Improved application performance by 50% through optimization
• Mentored team of 5 junior developers
• Implemented CI/CD pipelines reducing deployment time by 70%
• Collaborated with cross-functional teams on product features

Software Engineer | Previous Company | 2018 - 2020
• Developed and maintained enterprise applications
• Participated in code reviews and technical design discussions
• Contributed to agile development processes

EDUCATION
Bachelor of Science in Computer Science
University of Technology | 2015 - 2019
GPA: 3.8/4.0

CERTIFICATIONS
• AWS Certified Developer (2022)
• Microsoft Certified: Azure Developer Associate (2021)

---

COVER LETTER

Dear Hiring Manager,

I am writing to express my strong interest in the Senior Software Engineer position at TechCorp. With over 5 years of experience in full-stack development and a proven track record of delivering high-performance web applications, I am excited about the opportunity to contribute to your innovative team.

My experience aligns perfectly with your requirements. I have extensive experience with C#, JavaScript, and SQL, and have successfully led development teams to deliver scalable applications serving 100K+ users. My background in cloud technologies, including Azure and AWS, makes me well-suited for your hybrid work environment.

At my current role at Tech Corp, I have led development of scalable web applications serving 100K+ users, improved application performance by 50% through optimization, and mentored a team of 5 junior developers. I have also implemented CI/CD pipelines reducing deployment time by 70%.

I am particularly drawn to TechCorp's mission of building innovative software solutions and your values of innovation, collaboration, and excellence. I believe my technical expertise and collaborative approach would make me a valuable addition to your team.

I would welcome the opportunity to discuss how my skills and experience can contribute to TechCorp's continued success. Thank you for considering my application.

Best regards,
John Doe
";

            return await Task.FromResult(mockResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file with OpenAI: {FileId}", fileId);
            throw;
        }
    }
}