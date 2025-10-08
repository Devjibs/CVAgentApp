using System.ComponentModel.DataAnnotations;
using CVAgentApp.Core.Enums;

namespace CVAgentApp.Core.Entities;

public class JobPosting
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    public string? Responsibilities { get; set; }

    public string? Benefits { get; set; }

    public EmploymentType EmploymentType { get; set; }

    public ExperienceLevel ExperienceLevel { get; set; }

    public List<RequiredSkill> RequiredSkills { get; set; } = new();

    public List<RequiredQualification> RequiredQualifications { get; set; } = new();

    public CompanyInfo? CompanyInfo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class RequiredSkill
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public SkillLevel Level { get; set; }

    public bool IsRequired { get; set; }

    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
}

public class RequiredQualification
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public QualificationType Type { get; set; }

    public bool IsRequired { get; set; }

    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
}

public class CompanyInfo
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Mission { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Industry { get; set; }

    [MaxLength(200)]
    public string? Size { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    public List<string> Values { get; set; } = new();

    public List<string> RecentNews { get; set; } = new();

    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
}

