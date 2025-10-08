using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace CVAgentApp.Desktop.Services;

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

            // Generate mock documents
            var documents = new List<GeneratedDocumentDto>
            {
                new GeneratedDocumentDto
                {
                    Id = Guid.NewGuid(),
                    FileName = "Tailored_CV.pdf",
                    Type = DocumentType.CV,
                    Content = GenerateMockCVContent(),
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
                    Content = GenerateMockCoverLetterContent(),
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
                Message = "Mock documents generated successfully",
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
        return new SessionStatusResponse
        {
            SessionToken = sessionToken,
            Status = CVGenerationStatus.Completed,
            ProcessingLog = "Session completed"
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
        return new CandidateAnalysisResponse
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@email.com",
            Phone = "(555) 123-4567",
            Location = "San Francisco, CA",
            Summary = "Experienced software engineer with 5+ years of expertise in full-stack development",
            Skills = new List<SkillDto>
            {
                new SkillDto { Name = "C#", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 5 },
                new SkillDto { Name = "Python", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 3 },
                new SkillDto { Name = "JavaScript", Level = SkillLevel.Expert, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                new SkillDto { Name = "React", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 3 },
                new SkillDto { Name = "Azure", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 2 }
            },
            WorkExperiences = new List<WorkExperienceDto>
            {
                new WorkExperienceDto
                {
                    Company = "TechCorp Inc.",
                    Position = "Senior Software Engineer",
                    StartDate = DateTime.Parse("2021-01-01"),
                    EndDate = null,
                    IsCurrent = true,
                    Description = "Led development of microservices architecture serving 1M+ users"
                }
            },
            Education = new List<EducationDto>
            {
                new EducationDto
                {
                    Institution = "University of California, Berkeley",
                    Degree = "Bachelor of Science in Computer Science",
                    StartDate = DateTime.Parse("2015-09-01"),
                    EndDate = DateTime.Parse("2019-06-01")
                }
            }
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

    private string GenerateMockCVContent()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <title>John Doe - Software Engineer</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }
        .header { text-align: center; margin-bottom: 30px; }
        .section { margin-bottom: 20px; }
        .section h2 { color: #333; border-bottom: 2px solid #007acc; padding-bottom: 5px; }
        .contact-info { text-align: center; margin-bottom: 20px; }
        .experience-item { margin-bottom: 15px; }
        .company { font-weight: bold; color: #007acc; }
        .date { color: #666; font-style: italic; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>John Doe</h1>
        <div class='contact-info'>
            <p>📧 john.doe@email.com | 📱 (555) 123-4567 | 🌐 linkedin.com/in/johndoe</p>
            <p>📍 San Francisco, CA | 💼 Software Engineer</p>
        </div>
    </div>

    <div class='section'>
        <h2>Professional Summary</h2>
        <p>Experienced software engineer with 5+ years of expertise in full-stack development, cloud architecture, and team leadership. Passionate about building scalable applications and mentoring junior developers. Proven track record of delivering high-quality software solutions in agile environments.</p>
    </div>

    <div class='section'>
        <h2>Technical Skills</h2>
        <p><strong>Programming Languages:</strong> C#, Python, JavaScript, TypeScript, Java</p>
        <p><strong>Frameworks & Libraries:</strong> .NET Core, React, Angular, Node.js, ASP.NET MVC</p>
        <p><strong>Cloud & DevOps:</strong> Azure, AWS, Docker, Kubernetes, CI/CD, Terraform</p>
        <p><strong>Databases:</strong> SQL Server, PostgreSQL, MongoDB, Redis</p>
        <p><strong>Tools & Technologies:</strong> Git, Visual Studio, VS Code, Jira, Confluence</p>
    </div>

    <div class='section'>
        <h2>Professional Experience</h2>
        
        <div class='experience-item'>
            <div class='company'>Senior Software Engineer</div>
            <div>TechCorp Inc. | <span class='date'>2021 - Present</span></div>
            <ul>
                <li>Led development of microservices architecture serving 1M+ users</li>
                <li>Implemented CI/CD pipelines reducing deployment time by 60%</li>
                <li>Mentored 3 junior developers and conducted code reviews</li>
                <li>Designed and developed RESTful APIs using .NET Core and Azure</li>
            </ul>
        </div>

        <div class='experience-item'>
            <div class='company'>Software Engineer</div>
            <div>StartupXYZ | <span class='date'>2019 - 2021</span></div>
            <ul>
                <li>Built full-stack web applications using React and Node.js</li>
                <li>Collaborated with cross-functional teams in agile environment</li>
                <li>Optimized database queries improving performance by 40%</li>
                <li>Implemented automated testing increasing code coverage to 85%</li>
            </ul>
        </div>
    </div>

    <div class='section'>
        <h2>Education</h2>
        <p><strong>Bachelor of Science in Computer Science</strong></p>
        <p>University of California, Berkeley | <span class='date'>2015 - 2019</span></p>
        <p>GPA: 3.8/4.0 | Relevant Coursework: Data Structures, Algorithms, Software Engineering</p>
    </div>

    <div class='section'>
        <h2>Certifications</h2>
        <ul>
            <li>Microsoft Azure Solutions Architect Expert (2023)</li>
            <li>AWS Certified Developer Associate (2022)</li>
            <li>Certified Scrum Master (CSM) (2021)</li>
        </ul>
    </div>

    <div class='section'>
        <h2>Projects</h2>
        <p><strong>E-Commerce Platform</strong> - Built scalable e-commerce solution using .NET Core, React, and Azure</p>
        <p><strong>Real-time Chat Application</strong> - Developed using SignalR and WebSocket technologies</p>
        <p><strong>Machine Learning Pipeline</strong> - Created data processing pipeline using Python and Azure ML</p>
    </div>
</body>
</html>";
    }

    private string GenerateMockCoverLetterContent()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <title>Cover Letter - John Doe</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; max-width: 800px; }
        .header { text-align: right; margin-bottom: 30px; }
        .date { margin-bottom: 20px; }
        .greeting { margin-bottom: 20px; }
        .paragraph { margin-bottom: 15px; text-align: justify; }
        .closing { margin-top: 30px; }
    </style>
</head>
<body>
    <div class='header'>
        <p><strong>John Doe</strong></p>
        <p>john.doe@email.com | (555) 123-4567</p>
        <p>San Francisco, CA</p>
    </div>

    <div class='date'>
        <p>December 15, 2024</p>
    </div>

    <div class='greeting'>
        <p>Dear Hiring Manager,</p>
    </div>

    <div class='paragraph'>
        <p>I am writing to express my strong interest in the Software Engineer position at your company. With over 5 years of experience in full-stack development and a passion for building scalable applications, I am excited about the opportunity to contribute to your team's success.</p>
    </div>

    <div class='paragraph'>
        <p>In my current role as Senior Software Engineer at TechCorp Inc., I have led the development of microservices architecture that serves over 1 million users. I successfully implemented CI/CD pipelines that reduced deployment time by 60%, and I have mentored three junior developers while maintaining high code quality standards. My experience with .NET Core, Azure, and modern development practices aligns perfectly with your technology stack.</p>
    </div>

    <div class='paragraph'>
        <p>What particularly excites me about this opportunity is the chance to work on innovative projects that make a real impact. I have a proven track record of delivering high-quality software solutions in agile environments, and I am passionate about continuous learning and staying current with emerging technologies. My experience with cloud architecture, DevOps practices, and team collaboration makes me well-suited for this role.</p>
    </div>

    <div class='paragraph'>
        <p>I am particularly drawn to your company's commitment to innovation and growth. I would welcome the opportunity to discuss how my technical expertise and collaborative approach can contribute to your team's continued success. Thank you for considering my application.</p>
    </div>

    <div class='closing'>
        <p>Sincerely,</p>
        <p><strong>John Doe</strong></p>
    </div>
</body>
</html>";
    }
}
