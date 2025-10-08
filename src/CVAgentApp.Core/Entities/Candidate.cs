using System.ComponentModel.DataAnnotations;

namespace CVAgentApp.Core.Entities;

public class Candidate
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public string? Summary { get; set; }

    public List<WorkExperience> WorkExperiences { get; set; } = new();

    public List<Education> Education { get; set; } = new();

    public List<Skill> Skills { get; set; } = new();

    public List<Certification> Certifications { get; set; } = new();

    public List<Project> Projects { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class WorkExperience
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Position { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string? Description { get; set; }

    public List<string> Achievements { get; set; } = new();

    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}

public class Education
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Institution { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Degree { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FieldOfStudy { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? GPA { get; set; }

    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}

public class Skill
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public SkillLevel Level { get; set; }

    public SkillCategory Category { get; set; }

    public int YearsOfExperience { get; set; }

    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}

public class Certification
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? IssuingOrganization { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(100)]
    public string? CredentialId { get; set; }

    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}

public class Project
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(200)]
    public string? Url { get; set; }

    public List<string> Technologies { get; set; } = new();

    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}

public enum SkillLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}

public enum SkillCategory
{
    Technical = 1,
    Soft = 2,
    Language = 3,
    Industry = 4
}
