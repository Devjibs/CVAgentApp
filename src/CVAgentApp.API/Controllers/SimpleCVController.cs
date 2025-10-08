using Microsoft.AspNetCore.Mvc;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;

namespace CVAgentApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimpleCVController : ControllerBase
{
    private readonly ILogger<SimpleCVController> _logger;

    public SimpleCVController(ILogger<SimpleCVController> logger)
    {
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<CVGenerationResponse>> GenerateCV([FromForm] CVGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Received CV generation request");

            if (request.CVFile == null || request.CVFile.Length == 0)
            {
                return BadRequest("CV file is required");
            }

            if (string.IsNullOrEmpty(request.JobPostingUrl))
            {
                return BadRequest("Job posting URL is required");
            }

            // Simulate processing time
            await Task.Delay(2000);

            // Create mock response
            var response = new CVGenerationResponse
            {
                SessionId = Guid.NewGuid(),
                SessionToken = Guid.NewGuid().ToString(),
                Status = CVGenerationStatus.Completed,
                Message = "CV and cover letter generated successfully",
                Documents = new List<GeneratedDocumentDto>
                {
                    new GeneratedDocumentDto
                    {
                        Id = Guid.NewGuid(),
                        FileName = "Tailored_CV.pdf",
                        Type = DocumentType.CV,
                        Content = "Mock CV content...",
                        DownloadUrl = "http://localhost:5000/api/simplecv/download/cv",
                        FileSizeBytes = 1024,
                        Status = DocumentStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    },
                    new GeneratedDocumentDto
                    {
                        Id = Guid.NewGuid(),
                        FileName = "Cover_Letter.pdf",
                        Type = DocumentType.CoverLetter,
                        Content = "Mock cover letter content...",
                        DownloadUrl = "http://localhost:5000/api/simplecv/download/cover",
                        FileSizeBytes = 512,
                        Status = DocumentStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            _logger.LogInformation("CV generation request processed successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CV generation request");
            return StatusCode(500, new CVGenerationResponse
            {
                Status = CVGenerationStatus.Failed,
                ErrorMessage = "An error occurred while processing your request"
            });
        }
    }

    [HttpPost("analyze-job")]
    public async Task<ActionResult<JobAnalysisResponse>> AnalyzeJobPosting([FromBody] SimpleJobAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing job posting: {JobUrl}", request.JobUrl);

            if (string.IsNullOrEmpty(request.JobUrl))
            {
                return BadRequest("Job URL is required");
            }

            // Simulate processing time
            await Task.Delay(1000);

            // Create mock response
            var result = new JobAnalysisResponse
            {
                JobTitle = "Senior Software Engineer",
                Company = request.CompanyName ?? "Tech Company",
                Location = "Remote",
                EmploymentType = EmploymentType.FullTime,
                ExperienceLevel = ExperienceLevel.Senior,
                Description = "We are looking for a skilled software engineer to join our team.",
                Requirements = "Bachelor's degree in Computer Science, 3+ years experience",
                Responsibilities = "Develop and maintain software applications",
                RequiredSkills = new List<string> { "C#", "JavaScript", "SQL", "React" },
                RequiredQualifications = new List<string> { "Bachelor's degree", "3+ years experience" },
                CompanyInfo = new CompanyInfoDto
                {
                    Name = request.CompanyName ?? "Tech Company",
                    Mission = "To innovate and create amazing software",
                    Description = "A leading technology company",
                    Industry = "Technology",
                    Size = "100-500 employees",
                    Website = "https://example.com",
                    Values = new List<string> { "Innovation", "Collaboration", "Excellence" },
                    RecentNews = new List<string> { "Company raised Series A funding" }
                }
            };

            _logger.LogInformation("Job posting analysis completed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing job posting: {JobUrl}", request.JobUrl);
            return StatusCode(500, "An error occurred while analyzing the job posting");
        }
    }

    [HttpPost("analyze-candidate")]
    public async Task<ActionResult<CandidateAnalysisResponse>> AnalyzeCandidate([FromForm] IFormFile cvFile)
    {
        try
        {
            _logger.LogInformation("Analyzing candidate CV");

            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest("CV file is required");
            }

            // Simulate processing time
            await Task.Delay(1500);

            // Create mock response
            var result = new CandidateAnalysisResponse
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@email.com",
                Phone = "+1-555-0123",
                Location = "San Francisco, CA",
                Summary = "Experienced software engineer with 5+ years of experience",
                WorkExperiences = new List<WorkExperienceDto>
                {
                    new WorkExperienceDto
                    {
                        Company = "Tech Corp",
                        Position = "Senior Software Engineer",
                        StartDate = new DateTime(2020, 1, 1),
                        EndDate = null,
                        IsCurrent = true,
                        Description = "Led development of web applications",
                        Achievements = new List<string> { "Improved performance by 50%", "Led team of 5 developers" }
                    }
                },
                Education = new List<EducationDto>
                {
                    new EducationDto
                    {
                        Institution = "University of Technology",
                        Degree = "Bachelor of Science",
                        FieldOfStudy = "Computer Science",
                        StartDate = new DateTime(2015, 9, 1),
                        EndDate = new DateTime(2019, 6, 1),
                        GPA = 3.8m
                    }
                },
                Skills = new List<SkillDto>
                {
                    new SkillDto { Name = "C#", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 5 },
                    new SkillDto { Name = "JavaScript", Level = SkillLevel.Advanced, Category = SkillCategory.Technical, YearsOfExperience = 4 },
                    new SkillDto { Name = "SQL", Level = SkillLevel.Intermediate, Category = SkillCategory.Technical, YearsOfExperience = 3 }
                },
                Certifications = new List<CertificationDto>
                {
                    new CertificationDto
                    {
                        Name = "AWS Certified Developer",
                        IssuingOrganization = "Amazon Web Services",
                        IssueDate = new DateTime(2022, 1, 1),
                        ExpiryDate = new DateTime(2025, 1, 1),
                        CredentialId = "AWS-DEV-123456"
                    }
                },
                Projects = new List<ProjectDto>
                {
                    new ProjectDto
                    {
                        Name = "E-commerce Platform",
                        Description = "Full-stack web application for online shopping",
                        StartDate = new DateTime(2021, 1, 1),
                        EndDate = new DateTime(2021, 6, 1),
                        Url = "https://github.com/johndoe/ecommerce",
                        Technologies = new List<string> { "React", "Node.js", "MongoDB" }
                    }
                }
            };

            _logger.LogInformation("Candidate analysis completed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing candidate CV");
            return StatusCode(500, "An error occurred while analyzing the CV");
        }
    }

    [HttpGet("download/{type}")]
    public IActionResult DownloadDocument(string type)
    {
        try
        {
            _logger.LogInformation("Downloading document: {Type}", type);

            var content = type.ToLower() switch
            {
                "cv" => "Mock CV content - This would be the actual generated CV",
                "cover" => "Mock Cover Letter content - This would be the actual generated cover letter",
                _ => "Mock document content"
            };

            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var fileName = type.ToLower() switch
            {
                "cv" => "Tailored_CV.pdf",
                "cover" => "Cover_Letter.pdf",
                _ => "document.pdf"
            };

            return File(bytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {Type}", type);
            return StatusCode(500, "An error occurred while downloading the document");
        }
    }
}

public class SimpleJobAnalysisRequest
{
    public string JobUrl { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
}
