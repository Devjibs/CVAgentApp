using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CVAgentApp.Infrastructure.Services;

/// <summary>
/// OpenAI File service implementation
/// </summary>
public class OpenAIFileService : IOpenAIFileService
{
    private readonly IHttpClientService _httpClient;
    private readonly ILogger<OpenAIFileService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public OpenAIFileService(
        IHttpClientService httpClient,
        ILogger<OpenAIFileService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
        _baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
    }

    public async Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request)
    {
        try
        {
            _logger.LogInformation("Uploading file: {Filename}", request.Filename);

            var headers = GetDefaultHeaders();
            var content = CreateMultipartFormData(request);

            var response = await _httpClient.PostAsync($"{_baseUrl}/files", content, headers);
            response.EnsureSuccessStatusCode();

            var file = await _httpClient.PostAsync<OpenAIFile>($"{_baseUrl}/files", content, headers);

            _logger.LogInformation("File uploaded successfully: {FileId}", file.Id);

            return new FileUploadResponse
            {
                FileId = file.Id,
                Filename = file.Filename,
                Bytes = file.Bytes,
                Purpose = file.Purpose,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(file.CreatedAt).DateTime,
                ExpiresAt = file.ExpiresAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(file.ExpiresAt.Value).DateTime : null,
                Status = file.Status ?? "uploaded"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {Filename}", request.Filename);
            throw;
        }
    }

    public async Task<FileUploadResponse> UploadLargeFileAsync(FileUploadRequest request)
    {
        try
        {
            _logger.LogInformation("Uploading large file: {Filename}", request.Filename);

            // Create upload session
            var createUploadRequest = new CreateUploadRequest
            {
                Bytes = request.FileStream.Length,
                Filename = request.Filename,
                MimeType = GetMimeType(request.Filename),
                Purpose = request.Purpose,
                ExpiresAfter = request.ExpiresAfter
            };

            var upload = await CreateUploadAsync(createUploadRequest);
            var partIds = new List<string>();

            // Upload file in chunks
            const int chunkSize = 64 * 1024 * 1024; // 64MB chunks
            var buffer = new byte[chunkSize];
            int bytesRead;

            while ((bytesRead = await request.FileStream.ReadAsync(buffer, 0, chunkSize)) > 0)
            {
                using var chunkStream = new MemoryStream(buffer, 0, bytesRead);
                var part = await AddUploadPartAsync(upload.Id, chunkStream);
                partIds.Add(part.Id);
            }

            // Complete upload
            var completeRequest = new CompleteUploadRequest
            {
                PartIds = partIds
            };

            var completedUpload = await CompleteUploadAsync(upload.Id, completeRequest);

            _logger.LogInformation("Large file uploaded successfully: {FileId}", completedUpload.File?.Id);

            return new FileUploadResponse
            {
                FileId = completedUpload.File?.Id ?? string.Empty,
                Filename = completedUpload.Filename,
                Bytes = completedUpload.Bytes,
                Purpose = completedUpload.Purpose,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(completedUpload.CreatedAt).DateTime,
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(completedUpload.ExpiresAt).DateTime,
                Status = completedUpload.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading large file: {Filename}", request.Filename);
            throw;
        }
    }

    public async Task<OpenAIFileListResponse> ListFilesAsync(string? purpose = null, int limit = 100, string? after = null, string order = "desc")
    {
        try
        {
            _logger.LogInformation("Listing files with purpose: {Purpose}, limit: {Limit}", purpose, limit);

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(purpose))
                queryParams.Add($"purpose={purpose}");
            if (limit != 100)
                queryParams.Add($"limit={limit}");
            if (!string.IsNullOrEmpty(after))
                queryParams.Add($"after={after}");
            if (order != "desc")
                queryParams.Add($"order={order}");

            var endpoint = $"{_baseUrl}/files";
            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var headers = GetDefaultHeaders();
            var response = await _httpClient.GetAsync<OpenAIFileListResponse>(endpoint, headers);

            _logger.LogInformation("Retrieved {Count} files", response.Data.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            throw;
        }
    }

    public async Task<OpenAIFile> GetFileAsync(string fileId)
    {
        try
        {
            _logger.LogInformation("Getting file: {FileId}", fileId);

            var headers = GetDefaultHeaders();
            var file = await _httpClient.GetAsync<OpenAIFile>($"{_baseUrl}/files/{fileId}", headers);

            _logger.LogInformation("Retrieved file: {Filename}", file.Filename);
            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file: {FileId}", fileId);
            throw;
        }
    }

    public async Task<Stream> DownloadFileContentAsync(string fileId)
    {
        try
        {
            _logger.LogInformation("Downloading file content: {FileId}", fileId);

            var headers = GetDefaultHeaders();
            var response = await _httpClient.GetAsync($"{_baseUrl}/files/{fileId}/content", headers);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStreamAsync();
            _logger.LogInformation("Downloaded file content: {FileId}", fileId);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file content: {FileId}", fileId);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileId)
    {
        try
        {
            _logger.LogInformation("Deleting file: {FileId}", fileId);

            var headers = GetDefaultHeaders();
            var response = await _httpClient.DeleteAsync<FileDeletionResponse>($"{_baseUrl}/files/{fileId}", headers);

            _logger.LogInformation("File deleted: {FileId}, Success: {Success}", fileId, response.Deleted);
            return response.Deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileId}", fileId);
            throw;
        }
    }

    public async Task<OpenAIUpload> CreateUploadAsync(CreateUploadRequest request)
    {
        try
        {
            _logger.LogInformation("Creating upload: {Filename}", request.Filename);

            var headers = GetDefaultHeaders();
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var upload = await _httpClient.PostAsync<OpenAIUpload>($"{_baseUrl}/uploads", content, headers);

            _logger.LogInformation("Upload created: {UploadId}", upload.Id);
            return upload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating upload: {Filename}", request.Filename);
            throw;
        }
    }

    public async Task<OpenAIUploadPart> AddUploadPartAsync(string uploadId, Stream data)
    {
        try
        {
            _logger.LogInformation("Adding upload part: {UploadId}", uploadId);

            var headers = GetDefaultHeaders();
            var content = new StreamContent(data);

            var part = await _httpClient.PostAsync<OpenAIUploadPart>($"{_baseUrl}/uploads/{uploadId}/parts", content, headers);

            _logger.LogInformation("Upload part added: {PartId}", part.Id);
            return part;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding upload part: {UploadId}", uploadId);
            throw;
        }
    }

    public async Task<OpenAIUpload> CompleteUploadAsync(string uploadId, CompleteUploadRequest request)
    {
        try
        {
            _logger.LogInformation("Completing upload: {UploadId}", uploadId);

            var headers = GetDefaultHeaders();
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var upload = await _httpClient.PostAsync<OpenAIUpload>($"{_baseUrl}/uploads/{uploadId}/complete", content, headers);

            _logger.LogInformation("Upload completed: {UploadId}", uploadId);
            return upload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing upload: {UploadId}", uploadId);
            throw;
        }
    }

    public async Task<OpenAIUpload> CancelUploadAsync(string uploadId)
    {
        try
        {
            _logger.LogInformation("Cancelling upload: {UploadId}", uploadId);

            var headers = GetDefaultHeaders();
            var upload = await _httpClient.PostAsync<OpenAIUpload>($"{_baseUrl}/uploads/{uploadId}/cancel", headers);

            _logger.LogInformation("Upload cancelled: {UploadId}", uploadId);
            return upload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling upload: {UploadId}", uploadId);
            throw;
        }
    }

    private Dictionary<string, string> GetDefaultHeaders()
    {
        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {_apiKey}",
            ["OpenAI-Beta"] = "assistants=v2"
        };
    }

    private MultipartFormDataContent CreateMultipartFormData(FileUploadRequest request)
    {
        var content = new MultipartFormDataContent();
        
        // Add file stream
        var fileContent = new StreamContent(request.FileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(request.Filename));
        content.Add(fileContent, "file", request.Filename);

        // Add purpose
        content.Add(new StringContent(request.Purpose), "purpose");

        // Add expires_after if specified
        if (request.ExpiresAfter != null)
        {
            content.Add(new StringContent(request.ExpiresAfter.Anchor), "expires_after[anchor]");
            content.Add(new StringContent(request.ExpiresAfter.Seconds.ToString()), "expires_after[seconds]");
        }

        return content;
    }

    private string GetMimeType(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".jsonl" => "text/jsonl",
            ".csv" => "text/csv",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
