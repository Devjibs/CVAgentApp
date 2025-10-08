using System.ComponentModel.DataAnnotations;

namespace CVAgentApp.Core.Entities;

public class GeneratedDocument
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string OriginalFileName { get; set; } = string.Empty;

    public DocumentType Type { get; set; }

    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? BlobUrl { get; set; }

    [MaxLength(50)]
    public string? ContentType { get; set; }

    public long FileSizeBytes { get; set; }

    public DocumentStatus Status { get; set; }

    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;

    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;

    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? DownloadedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}

public class Session
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SessionToken { get; set; } = string.Empty;

    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;

    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;

    public SessionStatus Status { get; set; }

    public string? ProcessingLog { get; set; }

    public List<GeneratedDocument> GeneratedDocuments { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}

public enum DocumentType
{
    CV = 1,
    CoverLetter = 2,
    Portfolio = 3
}

public enum DocumentStatus
{
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Expired = 4
}

public enum SessionStatus
{
    Created = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Expired = 5
}
