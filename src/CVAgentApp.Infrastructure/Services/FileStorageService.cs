using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CVAgentApp.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _storagePath;

    public FileStorageService(ILogger<FileStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            _logger.LogInformation("Uploading file: {FileName}", fileName);

            var filePath = Path.Combine(_storagePath, fileName);

            using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamWriter);

            _logger.LogInformation("File uploaded successfully: {FileName}", fileName);
            return filePath;
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

            var fileStream = new FileStream(blobUrl, FileMode.Open, FileAccess.Read);

            _logger.LogInformation("File downloaded successfully: {BlobUrl}", blobUrl);
            return fileStream;
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

            if (File.Exists(blobUrl))
            {
                File.Delete(blobUrl);
                _logger.LogInformation("File deleted successfully: {BlobUrl}", blobUrl);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public async Task<string> GenerateDownloadUrlAsync(string blobUrl, TimeSpan expiry)
    {
        try
        {
            _logger.LogInformation("Generating download URL for: {BlobUrl}", blobUrl);

            // For local file storage, just return the file path
            // In a real implementation, you would generate a secure URL
            var downloadUrl = $"file://{blobUrl}";

            _logger.LogInformation("Download URL generated successfully");
            return downloadUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL: {BlobUrl}", blobUrl);
            throw;
        }
    }
}