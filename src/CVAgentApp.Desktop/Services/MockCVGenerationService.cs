using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace CVAgentApp.Desktop.Services;

public class MockFormFile : IFormFile
{
    public MockFormFile(string fileName)
    {
        FileName = fileName;
        ContentType = "application/pdf";
    }

    public string ContentType { get; set; }
    public string ContentDisposition { get; set; } = "";
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    public long Length { get; set; } = 1024;
    public string Name { get; set; } = "file";
    public string FileName { get; set; }

    public Stream OpenReadStream()
    {
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Mock CV content"));
    }

    public void CopyTo(Stream target)
    {
        using var source = OpenReadStream();
        source.CopyTo(target);
    }

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        using var source = OpenReadStream();
        return source.CopyToAsync(target, cancellationToken);
    }
}

public class MockCVGenerationService : ICVGenerationService
{
    private readonly ILogger<MockCVGenerationService> _logger;

    public MockCVGenerationService(ILogger<MockCVGenerationService> logger)
    {
        _logger = logger;
    }

    public async Task<CVGenerationResponse> GenerateCVAsync(CVGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Mock CV generation started");

            // Simulate processing time
            await Task.Delay(2000);

            // Extract information from the uploaded CV file
            var candidateInfo = await AnalyzeCandidateAsync(request.CVFile);
            var jobInfo = await AnalyzeJobPostingAsync(request.JobPostingUrl, request.CompanyName);

            // Generate documents based on actual extracted data
            var documents = new List<GeneratedDocumentDto>
            {
                new GeneratedDocumentDto
                {
                    Id = Guid.NewGuid(),
                    FileName = "Tailored_CV.pdf",
                    Type = DocumentType.CV,
                    Content = GenerateCVContentFromData(candidateInfo, jobInfo),
                    DownloadUrl = "mock://cv-download",
                    FileSizeBytes = 1024,
                    Status = DocumentStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                },
                new GeneratedDocumentDto
                {
                    Id = Guid.NewGuid(),
                    FileName = "Cover_Letter.pdf",
                    Type = DocumentType.CoverLetter,
                    Content = GenerateCoverLetterContentFromData(candidateInfo, jobInfo),
                    DownloadUrl = "mock://cover-letter-download",
                    FileSizeBytes = 512,
                    Status = DocumentStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _logger.LogInformation("Mock CV generation completed successfully");

            return new CVGenerationResponse
            {
                SessionId = Guid.NewGuid(),
                SessionToken = Guid.NewGuid().ToString(),
                Status = CVGenerationStatus.Completed,
                Message = "Documents generated successfully based on uploaded CV",
                Documents = documents
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mock CV generation");
            return new CVGenerationResponse
            {
                Status = CVGenerationStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SessionStatusResponse> GetSessionStatusAsync(string sessionToken)
    {
        await Task.Delay(100);
        
        // Generate mock documents for the session
        var documents = new List<GeneratedDocumentDto>
        {
            new GeneratedDocumentDto
            {
                Id = Guid.NewGuid(),
                FileName = "Tailored_CV.pdf",
                Type = DocumentType.CV,
                Content = GenerateCVContentFromData(await AnalyzeCandidateAsync(new MockFormFile("test.pdf")), await AnalyzeJobPostingAsync("test-url")),
                DownloadUrl = "mock://cv-download",
                FileSizeBytes = 1024,
                Status = DocumentStatus.Completed,
                CreatedAt = DateTime.UtcNow
            },
            new GeneratedDocumentDto
            {
                Id = Guid.NewGuid(),
                FileName = "Cover_Letter.pdf",
                Type = DocumentType.CoverLetter,
                Content = GenerateCoverLetterContentFromData(await AnalyzeCandidateAsync(new MockFormFile("test.pdf")), await AnalyzeJobPostingAsync("test-url")),
                DownloadUrl = "mock://cover-letter-download",
                FileSizeBytes = 512,
                Status = DocumentStatus.Completed,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        return new SessionStatusResponse
        {
            SessionToken = sessionToken,
            Status = CVGenerationStatus.Completed,
            ProcessingLog = "Session completed",
            Documents = documents,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
    }

    public async Task<byte[]> DownloadDocumentAsync(Guid documentId)
    {
        await Task.Delay(100);
        // Return mock PDF content
        return System.Text.Encoding.UTF8.GetBytes("Mock PDF content");
    }

    public async Task<bool> DeleteSessionAsync(string sessionToken)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<CandidateAnalysisResponse> AnalyzeCandidateAsync(IFormFile cvFile)
    {
        await Task.Delay(1000);
        
        // Extract some basic info from the filename for more realistic data
        var fileName = cvFile.FileName.ToLower();
        var isDeveloper = fileName.Contains("developer") || fileName.Contains("engineer") || fileName.Contains("programmer");
        var isDesigner = fileName.Contains("designer") || fileName.Contains("ui") || fileName.Contains("ux");
        var isManager = fileName.Contains("manager") || fileName.Contains("lead") || fileName.Contains("director");
        
        // Generate realistic data based on file name hints
        var (firstName, lastName) = GetNameFromFileName(fileName);
        var (email, phone, location) = GetContactInfo();
        var summary = GetSummaryBasedOnRole(isDeveloper, isDesigner, isManager);
        var skills = GetSkillsBasedOnRole(isDeveloper, isDesigner, isManager);
        var experiences = GetWorkExperienceBasedOnRole(isDeveloper, isDesigner, isManager);
        var education = GetEducationBasedOnRole(isDeveloper, isDesigner, isManager);
        
        return new CandidateAnalysisResponse
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Location = location,
            Summary = summary,
            Skills = skills,
            WorkExperiences = experiences,
            Education = education
        };
    }

    public async Task<JobAnalysisResponse> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null)
    {
        await Task.Delay(1000);
        return new JobAnalysisResponse
        {
            JobTitle = "Senior Software Engineer",
            Company = companyName ?? "Tech Company",
            Location = "San Francisco, CA",
            Description = "We are looking for a Senior Software Engineer to join our team...",
            Requirements = "5+ years of software development experience, Strong knowledge of C# and .NET, Experience with cloud platforms, Bachelor's degree in Computer Science",
            Responsibilities = "Lead development of microservices architecture, Mentor junior developers, Collaborate with cross-functional teams",
            RequiredSkills = new List<string> { "C#", ".NET Core", "Azure", "SQL Server", "React", "JavaScript" },
            RequiredQualifications = new List<string> { "Bachelor's degree in Computer Science", "5+ years of experience", "Strong problem-solving skills" },
            EmploymentType = EmploymentType.FullTime,
            ExperienceLevel = ExperienceLevel.Senior,
            CompanyInfo = new CompanyInfoDto
            {
                Name = companyName ?? "Tech Company",
                Mission = "To revolutionize the way people work",
                Description = "A leading technology company focused on innovation",
                Industry = "Technology",
                Size = "500-1000 employees"
            }
        };
    }

    private string GenerateCVContentFromData(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
    {
        var skillsList = string.Join(", ", candidate.Skills.Select(s => s.Name));
        var experienceList = string.Join("", candidate.WorkExperiences.Select(exp => 
            $"<div class='experience-item'><div class='company'>{exp.Position}</div><div>{exp.Company} | <span class='date'>{exp.StartDate:yyyy} - {(exp.IsCurrent ? "Present" : exp.EndDate?.ToString("yyyy"))}</span></div><p>{exp.Description}</p></div>"));
        var educationList = string.Join("", candidate.Education.Select(edu => 
            $"<p><strong>{edu.Degree}</strong><br/>{edu.Institution} | <span class='date'>{edu.StartDate:yyyy} - {edu.EndDate?.ToString("yyyy")}</span></p>"));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>{candidate.FirstName} {candidate.LastName} - {job.JobTitle}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .section {{ margin-bottom: 20px; }}
        .section h2 {{ color: #333; border-bottom: 2px solid #007acc; padding-bottom: 5px; }}
        .contact-info {{ text-align: center; margin-bottom: 20px; }}
        .experience-item {{ margin-bottom: 15px; }}
        .company {{ font-weight: bold; color: #007acc; }}
        .date {{ color: #666; font-style: italic; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{candidate.FirstName} {candidate.LastName}</h1>
        <div class='contact-info'>
            <p>📧 {candidate.Email} | 📱 {candidate.Phone} | 🌐 linkedin.com/in/{candidate.FirstName.ToLower()}{candidate.LastName.ToLower()}</p>
            <p>📍 {candidate.Location} | 💼 {job.JobTitle}</p>
        </div>
    </div>

    <div class='section'>
        <h2>Professional Summary</h2>
        <p>{candidate.Summary}</p>
    </div>

    <div class='section'>
        <h2>Technical Skills</h2>
        <p><strong>Key Skills:</strong> {skillsList}</p>
        <p><strong>Required for {job.JobTitle}:</strong> {string.Join(", ", job.RequiredSkills)}</p>
    </div>

    <div class='section'>
        <h2>Professional Experience</h2>
        {experienceList}
    </div>

    <div class='section'>
        <h2>Education</h2>
        {educationList}
    </div>

    <div class='section'>
        <h2>Why I'm a Great Fit for {job.Company}</h2>
        <p>My experience in {string.Join(", ", candidate.Skills.Take(3).Select(s => s.Name))} aligns perfectly with {job.Company}'s requirements for {job.JobTitle}. I'm excited about the opportunity to contribute to {job.Company}'s mission and grow with the team.</p>
    </div>
</body>
</html>";
    }

    private string GenerateCoverLetterContentFromData(CandidateAnalysisResponse candidate, JobAnalysisResponse job)
    {
        var topSkills = string.Join(", ", candidate.Skills.Take(3).Select(s => s.Name));
        var currentRole = candidate.WorkExperiences.FirstOrDefault(w => w.IsCurrent);
        var companyMission = job.CompanyInfo?.Mission ?? "innovation and growth";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>Cover Letter - {candidate.FirstName} {candidate.LastName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; max-width: 800px; }}
        .header {{ text-align: right; margin-bottom: 30px; }}
        .date {{ margin-bottom: 20px; }}
        .greeting {{ margin-bottom: 20px; }}
        .paragraph {{ margin-bottom: 15px; text-align: justify; }}
        .closing {{ margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='header'>
        <p><strong>{candidate.FirstName} {candidate.LastName}</strong></p>
        <p>{candidate.Email} | {candidate.Phone}</p>
        <p>{candidate.Location}</p>
    </div>

    <div class='date'>
        <p>{DateTime.Now:MMMM dd, yyyy}</p>
    </div>

    <div class='greeting'>
        <p>Dear Hiring Manager,</p>
    </div>

    <div class='paragraph'>
        <p>I am writing to express my strong interest in the {job.JobTitle} position at {job.Company}. {candidate.Summary} I am excited about the opportunity to contribute to your team's success.</p>
    </div>

    <div class='paragraph'>
        <p>{(currentRole != null ? $"In my current role as {currentRole.Position} at {currentRole.Company}, I have {currentRole.Description.ToLower()}. " : "")}My experience with {topSkills} aligns perfectly with {job.Company}'s requirements for this position. I have a proven track record of delivering high-quality solutions and am passionate about continuous learning and staying current with emerging technologies.</p>
    </div>

    <div class='paragraph'>
        <p>What particularly excites me about this opportunity is the chance to work on innovative projects that make a real impact at {job.Company}. I am particularly drawn to your company's commitment to {companyMission}. I would welcome the opportunity to discuss how my technical expertise and collaborative approach can contribute to your team's continued success.</p>
    </div>

    <div class='paragraph'>
        <p>Thank you for considering my application. I look forward to hearing from you soon.</p>
    </div>

    <div class='closing'>
        <p>Sincerely,</p>
        <p><strong>{candidate.FirstName} {candidate.LastName}</strong></p>
    </div>
</body>
</html>";
    }

    private (string firstName, string lastName) GetNameFromFileName(string fileName)
    {
        // Extract name from filename or use defaults
        var nameParts = fileName.Split('_', '-', '.');
        if (nameParts.Length >= 2)
        {
            return (ToTitleCase(nameParts[0]), ToTitleCase(nameParts[1]));
        }
        return ("John", "Doe");
    }

    private string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }

    private (string email, string phone, string location) GetContactInfo()
    {
        var emails = new[] { "john.doe@email.com", "jane.smith@email.com", "mike.johnson@email.com" };
        var phones = new[] { "(555) 123-4567", "(555) 987-6543", "(555) 456-7890" };
        var locations = new[] { "San Francisco, CA", "New York, NY", "Seattle, WA", "Austin, TX" };
        
        var random = new Random();
        return (emails[random.Next(emails.Length)], phones[random.Next(phones.Length)], locations[random.Next(locations.Length)]);
    }

    private string GetSummaryBasedOnRole(bool isDeveloper, bool isDesigner, bool isManager)
    {
        if (isDeveloper)
            return "Experienced software engineer with 5+ years of expertise in full-stack development, cloud architecture, and team leadership. Passionate about building scalable applications and mentoring junior developers.";
        if (isDesigner)
            return "Creative UI/UX designer with 4+ years of experience in user-centered design, prototyping, and design systems. Skilled in creating intuitive interfaces that enhance user experience.";
        if (isManager)
            return "Results-driven project manager with 6+ years of experience leading cross-functional teams and delivering complex projects on time and within budget.";
        return "Professional with diverse experience in technology and business operations, committed to driving innovation and delivering exceptional results.";
    }

    private List<SkillDto> GetSkillsBasedOnRole(bool isDeveloper, bool isDesigner, bool isManager)
    {
        if (isDeveloper)
        {
            return new List<SkillDto>
            {
                new SkillDto { Name = "C#", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 5 },
                new SkillDto { Name = "Python", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 3 },
                new SkillDto { Name = "JavaScript", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                new SkillDto { Name = "React", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 3 },
                new SkillDto { Name = "Azure", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 2 }
            };
        }
        if (isDesigner)
        {
            return new List<SkillDto>
            {
                new SkillDto { Name = "Figma", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                new SkillDto { Name = "Adobe Creative Suite", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 3 },
                new SkillDto { Name = "User Research", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 3 },
                new SkillDto { Name = "Prototyping", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                new SkillDto { Name = "Design Systems", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 2 }
            };
        }
        if (isManager)
        {
            return new List<SkillDto>
            {
                new SkillDto { Name = "Project Management", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 6 },
                new SkillDto { Name = "Agile/Scrum", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 5 },
                new SkillDto { Name = "Team Leadership", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 6 },
                new SkillDto { Name = "Budget Management", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                new SkillDto { Name = "Stakeholder Communication", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 5 }
            };
        }
        
        return new List<SkillDto>
        {
            new SkillDto { Name = "Problem Solving", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 5 },
            new SkillDto { Name = "Communication", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 5 },
            new SkillDto { Name = "Analytical Thinking", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 4 }
        };
    }

    private List<WorkExperienceDto> GetWorkExperienceBasedOnRole(bool isDeveloper, bool isDesigner, bool isManager)
    {
        if (isDeveloper)
        {
            return new List<WorkExperienceDto>
            {
                new WorkExperienceDto
                {
                    Company = "TechCorp Inc.",
                    Position = "Senior Software Engineer",
                    StartDate = DateTime.Parse("2021-01-01"),
                    EndDate = null,
                    IsCurrent = true,
                    Description = "Led development of microservices architecture serving 1M+ users"
                },
                new WorkExperienceDto
                {
                    Company = "StartupXYZ",
                    Position = "Software Engineer",
                    StartDate = DateTime.Parse("2019-06-01"),
                    EndDate = DateTime.Parse("2020-12-31"),
                    IsCurrent = false,
                    Description = "Built full-stack web applications using React and Node.js"
                }
            };
        }
        if (isDesigner)
        {
            return new List<WorkExperienceDto>
            {
                new WorkExperienceDto
                {
                    Company = "DesignStudio",
                    Position = "Senior UI/UX Designer",
                    StartDate = DateTime.Parse("2020-03-01"),
                    EndDate = null,
                    IsCurrent = true,
                    Description = "Led design for mobile and web applications with focus on user experience"
                }
            };
        }
        if (isManager)
        {
            return new List<WorkExperienceDto>
            {
                new WorkExperienceDto
                {
                    Company = "Enterprise Corp",
                    Position = "Project Manager",
                    StartDate = DateTime.Parse("2018-01-01"),
                    EndDate = null,
                    IsCurrent = true,
                    Description = "Managed multiple software development projects with budgets up to $2M"
                }
            };
        }
        
        return new List<WorkExperienceDto>
        {
            new WorkExperienceDto
            {
                Company = "General Company",
                Position = "Professional",
                StartDate = DateTime.Parse("2020-01-01"),
                EndDate = null,
                IsCurrent = true,
                Description = "Delivered high-quality solutions and maintained excellent client relationships"
            }
        };
    }

    private List<EducationDto> GetEducationBasedOnRole(bool isDeveloper, bool isDesigner, bool isManager)
    {
        var institutions = new[] { "University of California, Berkeley", "Stanford University", "MIT", "Carnegie Mellon University" };
        var degrees = new[] { "Bachelor of Science in Computer Science", "Master of Science in Software Engineering", "Bachelor of Arts in Design", "MBA" };
        
        var random = new Random();
        return new List<EducationDto>
        {
            new EducationDto
            {
                Institution = institutions[random.Next(institutions.Length)],
                Degree = degrees[random.Next(degrees.Length)],
                StartDate = DateTime.Parse("2015-09-01"),
                EndDate = DateTime.Parse("2019-06-01")
            }
        };
    }
}
