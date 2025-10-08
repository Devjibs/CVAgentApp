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
    private readonly string _apiKey;

    public OpenAIService(ILogger<OpenAIService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
    }

    public Task<string> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null)
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

            // For now, return a mock response since we need to fix the OpenAI package
            var mockResponse = new JobAnalysisResponse
            {
                JobTitle = "Senior .NET Developer",
                Company = companyName ?? "Hays",
                Location = "London, UK",
                EmploymentType = EmploymentType.FullTime,
                ExperienceLevel = ExperienceLevel.Senior,
                Description = "We are looking for a skilled Senior .NET Developer to join our team. You will be responsible for developing and maintaining web applications using C#, ASP.NET Core, and related technologies.",
                Requirements = "Bachelor's degree in Computer Science, 5+ years experience with .NET, C#, ASP.NET Core, Entity Framework, SQL Server",
                Responsibilities = "Develop and maintain web applications, work with cross-functional teams, mentor junior developers",
                RequiredSkills = new List<string> { "C#", "ASP.NET Core", "Entity Framework", "SQL Server", "JavaScript", "React", "Azure" },
                RequiredQualifications = new List<string> { "Bachelor's degree in Computer Science", "5+ years experience with .NET" },
                CompanyInfo = new CompanyInfoDto
                {
                    Name = companyName ?? "Hays",
                    Mission = "To connect talented people with great opportunities",
                    Description = "A leading recruitment company specializing in technology roles",
                    Industry = "Recruitment & Technology",
                    Size = "1000+ employees",
                    Website = "https://www.hays.co.uk",
                    Values = new List<string> { "Excellence", "Integrity", "Innovation", "Collaboration" },
                    RecentNews = new List<string> { "Expanding technology division", "New office opening in London" }
                }
            };

            _logger.LogInformation("Job posting analysis completed");
            return Task.FromResult(JsonSerializer.Serialize(mockResponse));
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

            // For now, return a mock response
            var mockResponse = new CandidateAnalysisResponse
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@email.com",
                Phone = "+44 1234 567890",
                Location = "London, UK",
                Summary = "Senior .NET Developer with 5+ years of experience in developing scalable web applications using C#, ASP.NET Core, and cloud technologies.",
                WorkExperiences = new List<WorkExperienceDto>
                {
                    new WorkExperienceDto
                    {
                        Company = "TechCorp Ltd",
                        Position = "Senior .NET Developer",
                        StartDate = new DateTime(2020, 1, 1),
                        EndDate = null,
                        IsCurrent = true,
                        Description = "Led development of scalable web applications using C#, ASP.NET Core, and Entity Framework. Managed a team of 5 developers and implemented microservices architecture.",
                        Achievements = new List<string> { "Improved application performance by 50%", "Led team of 5 developers", "Implemented CI/CD pipelines" }
                    },
                    new WorkExperienceDto
                    {
                        Company = "StartupXYZ",
                        Position = ".NET Developer",
                        StartDate = new DateTime(2018, 6, 1),
                        EndDate = new DateTime(2019, 12, 31),
                        IsCurrent = false,
                        Description = "Developed RESTful APIs using ASP.NET Web API and worked with SQL Server and Entity Framework Core.",
                        Achievements = new List<string> { "Built 10+ RESTful APIs", "Improved database performance by 30%" }
                    }
                },
                Education = new List<EducationDto>
                {
                    new EducationDto
                    {
                        Institution = "University of Technology",
                        Degree = "Bachelor of Science",
                        FieldOfStudy = "Computer Science",
                        StartDate = new DateTime(2014, 9, 1),
                        EndDate = new DateTime(2018, 6, 1),
                        GPA = 3.8m
                    }
                },
                Skills = new List<SkillDto>
                {
                    new SkillDto { Name = "C#", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 5 },
                    new SkillDto { Name = "ASP.NET Core", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                    new SkillDto { Name = "Entity Framework", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                    new SkillDto { Name = "SQL Server", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                    new SkillDto { Name = "JavaScript", Level = SkillLevel.Intermediate, Category = SkillCategory.Technical, YearsOfExperience = 3 },
                    new SkillDto { Name = "Azure", Level = SkillLevel.Intermediate, Category = SkillCategory.Technical, YearsOfExperience = 2 }
                },
                Certifications = new List<CertificationDto>
                {
                    new CertificationDto
                    {
                        Name = "Microsoft Certified: Azure Developer Associate",
                        IssuingOrganization = "Microsoft",
                        IssueDate = new DateTime(2022, 1, 15),
                        ExpiryDate = new DateTime(2024, 1, 15),
                        CredentialId = "AZ-204"
                    }
                },
                Projects = new List<ProjectDto>
                {
                    new ProjectDto
                    {
                        Name = "E-commerce Platform",
                        Description = "Built a full-stack e-commerce platform using .NET Core, React, and Azure",
                        StartDate = new DateTime(2021, 1, 1),
                        EndDate = new DateTime(2021, 6, 1),
                        Url = "https://github.com/johnsmith/ecommerce",
                        Technologies = new List<string> { "C#", "ASP.NET Core", "React", "SQL Server", "Azure" }
                    }
                }
            };

            _logger.LogInformation("CV analysis completed");
            return Task.FromResult(JsonSerializer.Serialize(mockResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing candidate CV");
            throw;
        }
    }

    public Task<string> GenerateCVAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
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

            // For now, return a mock CV
            var mockCV = $@"
JOHN DOE
Senior Software Engineer
Email: john.doe@email.com | Phone: +1-555-0123 | Location: San Francisco, CA

PROFESSIONAL SUMMARY
Experienced software engineer with 5+ years of experience in full-stack development, 
specializing in C# and JavaScript. Proven track record of leading development teams 
and delivering high-performance web applications. Strong background in {job.JobTitle} 
role with expertise in {string.Join(", ", job.RequiredSkills.Take(3))}.

TECHNICAL SKILLS
• Programming Languages: C#, JavaScript, SQL
• Frameworks: .NET Core, React, Node.js
• Databases: SQL Server, MongoDB
• Cloud Platforms: AWS, Azure
• Tools: Git, Docker, Jenkins

PROFESSIONAL EXPERIENCE

Senior Software Engineer | Tech Corp | 2020 - Present
• Led development of scalable web applications serving 100K+ users
• Improved application performance by 50% through optimization
• Mentored team of 5 junior developers
• Implemented CI/CD pipelines reducing deployment time by 70%

Software Engineer | Previous Company | 2018 - 2020
• Developed and maintained enterprise applications
• Collaborated with cross-functional teams on product features
• Participated in code reviews and technical design discussions

EDUCATION
Bachelor of Science in Computer Science
University of Technology | 2015 - 2019
GPA: 3.8/4.0

CERTIFICATIONS
• AWS Certified Developer (2022)
• Microsoft Certified: Azure Developer Associate (2021)

PROJECTS
E-commerce Platform (2021)
• Full-stack web application for online shopping
• Technologies: React, Node.js, MongoDB
• GitHub: https://github.com/johndoe/ecommerce
";

            _logger.LogInformation("CV generation completed");
            return Task.FromResult(mockCV);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CV");
            throw;
        }
    }

    public Task<string> GenerateCoverLetterAsync(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
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

            // For now, return a mock cover letter
            var mockCoverLetter = $@"
Dear Hiring Manager,

I am writing to express my strong interest in the {job.JobTitle} position at {job.Company}. 
With over 5 years of experience in software development and a proven track record of 
delivering high-quality solutions, I am excited about the opportunity to contribute 
to your team.

My experience with {string.Join(", ", candidate.Skills.Take(3).Select(s => s.Name))} aligns 
perfectly with the requirements for this role. At Tech Corp, I have successfully led 
development teams and implemented scalable solutions that improved performance by 50%. 
I am particularly drawn to {job.Company}'s mission and believe my technical expertise 
and passion for innovation would make me a valuable addition to your team.

I am enthusiastic about the opportunity to discuss how my skills and experience can 
contribute to {job.Company}'s continued success. Thank you for considering my application.

Sincerely,
{candidate.FirstName} {candidate.LastName}
";

            _logger.LogInformation("Cover letter generation completed");
            return Task.FromResult(mockCoverLetter);
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

            // For now, return a mock response
            var mockResponse = new
            {
                name = companyName,
                mission = "To innovate and create amazing software solutions",
                description = "A leading technology company focused on digital transformation",
                industry = "Technology",
                size = "100-500 employees",
                website = "https://example.com",
                values = new[] { "Innovation", "Collaboration", "Excellence", "Integrity" },
                recentNews = new[] { "Company raised Series A funding", "Launched new AI product line" }
            };

            _logger.LogInformation("Company research completed for: {CompanyName}", companyName);
            return Task.FromResult(JsonSerializer.Serialize(mockResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error researching company: {CompanyName}", companyName);
            throw;
        }
    }
}