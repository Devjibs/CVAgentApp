using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using CVAgentApp.Core.Entities;

namespace CVAgentApp.Core.DTOs;

public class CVGenerationRequest
{
    [Required]
    public IFormFile CVFile { get; set; } = null!;

    [Required]
    [Url]
    public string JobPostingUrl { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public string? SessionToken { get; set; }
}

public class CVGenerationResponse
{
    public Guid SessionId { get; set; }

    public string SessionToken { get; set; } = string.Empty;

    public CVGenerationStatus Status { get; set; }

    public string? Message { get; set; }

    public List<GeneratedDocumentDto> Documents { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

public class GeneratedDocumentDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public DocumentType Type { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? DownloadUrl { get; set; }

    public long FileSizeBytes { get; set; }

    public DocumentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class SessionStatusResponse
{
    public Guid SessionId { get; set; }

    public string SessionToken { get; set; } = string.Empty;

    public CVGenerationStatus Status { get; set; }

    public string? ProcessingLog { get; set; }

    public List<GeneratedDocumentDto> Documents { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}

public class JobAnalysisResponse
{
    public string JobTitle { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public EmploymentType EmploymentType { get; set; }

    public ExperienceLevel ExperienceLevel { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;

    public string Responsibilities { get; set; } = string.Empty;

    public List<string> RequiredSkills { get; set; } = new();

    public List<string> RequiredQualifications { get; set; } = new();

    public CompanyInfoDto? CompanyInfo { get; set; }
}

public class CompanyInfoDto
{
    public string Name { get; set; } = string.Empty;

    public string? Mission { get; set; }

    public string? Description { get; set; }

    public string? Industry { get; set; }

    public string? Size { get; set; }

    public string? Website { get; set; }

    public List<string> Values { get; set; } = new();

    public List<string> RecentNews { get; set; } = new();
}

public class CandidateAnalysisResponse
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Location { get; set; }

    public string? Summary { get; set; }

    public List<WorkExperienceDto> WorkExperiences { get; set; } = new();

    public List<EducationDto> Education { get; set; } = new();

    public List<SkillDto> Skills { get; set; } = new();

    public List<CertificationDto> Certifications { get; set; } = new();

    public List<ProjectDto> Projects { get; set; } = new();
}

public class WorkExperienceDto
{
    public string Company { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string? Description { get; set; }

    public List<string> Achievements { get; set; } = new();
}

public class EducationDto
{
    public string Institution { get; set; } = string.Empty;

    public string Degree { get; set; } = string.Empty;

    public string? FieldOfStudy { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? GPA { get; set; }
}

public class SkillDto
{
    public string Name { get; set; } = string.Empty;

    public SkillLevel Level { get; set; }

    public SkillCategory Category { get; set; }

    public int YearsOfExperience { get; set; }
}

public class CertificationDto
{
    public string Name { get; set; } = string.Empty;

    public string? IssuingOrganization { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? CredentialId { get; set; }
}

public class ProjectDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Url { get; set; }

    public List<string> Technologies { get; set; } = new();
}

public enum CVGenerationStatus
{
    Created = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Expired = 5
}
