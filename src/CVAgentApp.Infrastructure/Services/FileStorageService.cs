using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CVAgentApp.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly IOpenAIFileService _openAIFileService;
    private readonly string _storagePath;
    private readonly bool _useOpenAI;

    public FileStorageService(
        ILogger<FileStorageService> logger, 
        IConfiguration configuration,
        IOpenAIFileService openAIFileService)
    {
        _logger = logger;
        _openAIFileService = openAIFileService;
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        _useOpenAI = configuration.GetValue<bool>("FileStorage:UseOpenAI", true);
        
        if (!_useOpenAI)
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            _logger.LogInformation("Uploading file: {FileName}", fileName);

            if (_useOpenAI)
            {
                // Use OpenAI file service
                var request = new FileUploadRequest
                {
                    FileStream = fileStream,
                    Filename = fileName,
                    Purpose = "assistants", // Default purpose for CV files
                    ExpiresAfter = new ExpiresAfter
                    {
                        Anchor = "created_at",
                        Seconds = 30 * 24 * 60 * 60 // 30 days
                    }
                };

                var response = await _openAIFileService.UploadFileAsync(request);
                _logger.LogInformation("File uploaded to OpenAI successfully: {FileId}", response.FileId);
                return response.FileId;
            }
            else
            {
                // Use local file storage
                var filePath = Path.Combine(_storagePath, fileName);

                using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
                await fileStream.CopyToAsync(fileStreamWriter);

                _logger.LogInformation("File uploaded to local storage successfully: {FileName}", fileName);
                return filePath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string blobUrl)
    {
        try
        {
            _logger.LogInformation("Downloading file: {BlobUrl}", blobUrl);

            if (_useOpenAI && IsOpenAIFileId(blobUrl))
            {
                // Use OpenAI file service
                var content = await _openAIFileService.DownloadFileContentAsync(blobUrl);
                _logger.LogInformation("File downloaded from OpenAI successfully: {FileId}", blobUrl);
                return content;
            }
            else
            {
                // Use local file storage
                var fileStream = new FileStream(blobUrl, FileMode.Open, FileAccess.Read);
                _logger.LogInformation("File downloaded from local storage successfully: {BlobUrl}", blobUrl);
                return fileStream;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string blobUrl)
    {
        try
        {
            _logger.LogInformation("Deleting file: {BlobUrl}", blobUrl);

            if (_useOpenAI && IsOpenAIFileId(blobUrl))
            {
                // Use OpenAI file service
                var success = await _openAIFileService.DeleteFileAsync(blobUrl);
                _logger.LogInformation("File deleted from OpenAI successfully: {FileId}", blobUrl);
                return success;
            }
            else
            {
                // Use local file storage
                if (File.Exists(blobUrl))
                {
                    File.Delete(blobUrl);
                    _logger.LogInformation("File deleted from local storage successfully: {BlobUrl}", blobUrl);
                    return true;
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public Task<string> GenerateDownloadUrlAsync(string blobUrl, TimeSpan expiry)
    {
        try
        {
            _logger.LogInformation("Generating download URL for: {BlobUrl}", blobUrl);

            if (_useOpenAI && IsOpenAIFileId(blobUrl))
            {
                // For OpenAI files, we can't generate direct download URLs
                // The file ID itself serves as the reference
                _logger.LogInformation("OpenAI file ID generated successfully: {FileId}", blobUrl);
                return Task.FromResult(blobUrl);
            }
            else
            {
                // For local file storage, just return the file path
                // In a real implementation, you would generate a secure URL
                var downloadUrl = $"file://{blobUrl}";
                _logger.LogInformation("Download URL generated successfully");
                return Task.FromResult(downloadUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL: {BlobUrl}", blobUrl);
            throw;
        }
    }

    private bool IsOpenAIFileId(string blobUrl)
    {
        // OpenAI file IDs typically start with "file-" and are alphanumeric
        return blobUrl.StartsWith("file-") && blobUrl.Length > 5;
    }
}