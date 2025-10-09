using System.Text.Json.Serialization;

namespace CVAgentApp.Core.DTOs;

/// <summary>
/// OpenAI File object
/// </summary>
public class OpenAIFile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = "file";

    [JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("status_details")]
    public string? StatusDetails { get; set; }
}

/// <summary>
/// OpenAI File list response
/// </summary>
public class OpenAIFileListResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("data")]
    public List<OpenAIFile> Data { get; set; } = new();

    [JsonPropertyName("first_id")]
    public string? FirstId { get; set; }

    [JsonPropertyName("last_id")]
    public string? LastId { get; set; }

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }
}

/// <summary>
/// OpenAI Upload object
/// </summary>
public class OpenAIUpload
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = "upload";

    [JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("file")]
    public OpenAIFile? File { get; set; }
}

/// <summary>
/// OpenAI Upload Part object
/// </summary>
public class OpenAIUploadPart
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = "upload.part";

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("upload_id")]
    public string UploadId { get; set; } = string.Empty;
}

/// <summary>
/// Create upload request
/// </summary>
public class CreateUploadRequest
{
    [JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("expires_after")]
    public ExpiresAfter? ExpiresAfter { get; set; }
}

/// <summary>
/// Expires after configuration
/// </summary>
public class ExpiresAfter
{
    [JsonPropertyName("anchor")]
    public string Anchor { get; set; } = "created_at";

    [JsonPropertyName("seconds")]
    public long Seconds { get; set; }
}

/// <summary>
/// Complete upload request
/// </summary>
public class CompleteUploadRequest
{
    [JsonPropertyName("part_ids")]
    public List<string> PartIds { get; set; } = new();

    [JsonPropertyName("md5")]
    public string? Md5 { get; set; }
}

/// <summary>
/// File deletion response
/// </summary>
public class FileDeletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = "file";

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
}

/// <summary>
/// File upload request for simple uploads
/// </summary>
public class FileUploadRequest
{
    public Stream FileStream { get; set; } = Stream.Null;
    public string Filename { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public ExpiresAfter? ExpiresAfter { get; set; }
}

/// <summary>
/// File upload response
/// </summary>
public class FileUploadResponse
{
    public string FileId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}


