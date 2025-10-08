using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
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
            var mockResponse = new
            {
                jobTitle = "Software Engineer",
                company = companyName ?? "Tech Company",
                location = "Remote",
                employmentType = "FullTime",
                experienceLevel = "Mid",
                description = "We are looking for a skilled software engineer to join our team.",
                requirements = "Bachelor's degree in Computer Science, 3+ years experience",
                responsibilities = "Develop and maintain software applications",
                requiredSkills = new[] { "C#", "JavaScript", "SQL" },
                requiredQualifications = new[] { "Bachelor's degree", "3+ years experience" },
                companyInfo = new
                {
                    mission = "To innovate and create amazing software",
                    description = "A leading technology company",
                    industry = "Technology",
                    size = "100-500 employees",
                    website = "https://example.com",
                    values = new[] { "Innovation", "Collaboration", "Excellence" },
                    recentNews = new[] { "Company raised Series A funding" }
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
            var mockResponse = new
            {
                firstName = "John",
                lastName = "Doe",
                email = "john.doe@email.com",
                phone = "+1-555-0123",
                location = "San Francisco, CA",
                summary = "Experienced software engineer with 5+ years of experience",
                workExperiences = new[]
                {
                    new
                    {
                        company = "Tech Corp",
                        position = "Senior Software Engineer",
                        startDate = "2020-01-01",
                        endDate = (string?)null,
                        isCurrent = true,
                        description = "Led development of web applications",
                        achievements = new[] { "Improved performance by 50%", "Led team of 5 developers" }
                    }
                },
                education = new[]
                {
                    new
                    {
                        institution = "University of Technology",
                        degree = "Bachelor of Science",
                        fieldOfStudy = "Computer Science",
                        startDate = "2015-09-01",
                        endDate = "2019-06-01",
                        gpa = 3.8m
                    }
                },
                skills = new[]
                {
                    new { name = "C#", level = "Advanced", category = "Technical", yearsOfExperience = 5 },
                    new { name = "JavaScript", level = "Advanced", category = "Technical", yearsOfExperience = 4 },
                    new { name = "SQL", level = "Intermediate", category = "Technical", yearsOfExperience = 3 }
                },
                certifications = new[]
                {
                    new
                    {
                        name = "AWS Certified Developer",
                        issuingOrganization = "Amazon Web Services",
                        issueDate = "2022-01-01",
                        expiryDate = "2025-01-01",
                        credentialId = "AWS-DEV-123456"
                    }
                },
                projects = new[]
                {
                    new
                    {
                        name = "E-commerce Platform",
                        description = "Full-stack web application for online shopping",
                        startDate = "2021-01-01",
                        endDate = "2021-06-01",
                        url = "https://github.com/johndoe/ecommerce",
                        technologies = new[] { "React", "Node.js", "MongoDB" }
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