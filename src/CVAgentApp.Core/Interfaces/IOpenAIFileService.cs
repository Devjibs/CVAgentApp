using CVAgentApp.Core.DTOs;

namespace CVAgentApp.Core.Interfaces;

/// <summary>
/// OpenAI File service interface for file operations
/// </summary>
public interface IOpenAIFileService
{
    /// <summary>
    /// Upload a file to OpenAI
    /// </summary>
    Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request);

    /// <summary>
    /// Upload a large file using multipart upload
    /// </summary>
    Task<FileUploadResponse> UploadLargeFileAsync(FileUploadRequest request);

    /// <summary>
    /// List files with optional filtering
    /// </summary>
    Task<OpenAIFileListResponse> ListFilesAsync(string? purpose = null, int limit = 100, string? after = null, string order = "desc");

    /// <summary>
    /// Get file information by ID
    /// </summary>
    Task<OpenAIFile> GetFileAsync(string fileId);

    /// <summary>
    /// Download file content by ID
    /// </summary>
    Task<Stream> DownloadFileContentAsync(string fileId);

    /// <summary>
    /// Delete a file by ID
    /// </summary>
    Task<bool> DeleteFileAsync(string fileId);

    /// <summary>
    /// Create an upload session for large files
    /// </summary>
    Task<OpenAIUpload> CreateUploadAsync(CreateUploadRequest request);

    /// <summary>
    /// Add a part to an upload
    /// </summary>
    Task<OpenAIUploadPart> AddUploadPartAsync(string uploadId, Stream data);

    /// <summary>
    /// Complete an upload
    /// </summary>
    Task<OpenAIUpload> CompleteUploadAsync(string uploadId, CompleteUploadRequest request);

    /// <summary>
    /// Cancel an upload
    /// </summary>
    Task<OpenAIUpload> CancelUploadAsync(string uploadId);
}


