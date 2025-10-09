using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CVAgentApp.API.Controllers;

/// <summary>
/// Test controller for OpenAI file integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileTestController : ControllerBase
{
    private readonly ILogger<FileTestController> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly IOpenAIFileService _openAIFileService;

    public FileTestController(
        ILogger<FileTestController> logger,
        IFileStorageService fileStorageService,
        IOpenAIFileService openAIFileService)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
        _openAIFileService = openAIFileService;
    }

    /// <summary>
    /// Test file upload
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<FileUploadResponse>> UploadFile([FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            _logger.LogInformation("Testing file upload: {FileName}", file.FileName);

            using var stream = file.OpenReadStream();
            var fileId = await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

            return Ok(new FileUploadResponse
            {
                FileId = fileId,
                Filename = file.FileName,
                Bytes = file.Length,
                Purpose = "assistants",
                CreatedAt = DateTime.UtcNow,
                Status = "uploaded"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing file upload");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Test file download
    /// </summary>
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        try
        {
            _logger.LogInformation("Testing file download: {FileId}", fileId);

            var stream = await _fileStorageService.DownloadFileAsync(fileId);
            
            return File(stream, "application/octet-stream", $"downloaded_{fileId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing file download: {FileId}", fileId);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Test file deletion
    /// </summary>
    [HttpDelete("{fileId}")]
    public async Task<ActionResult<bool>> DeleteFile(string fileId)
    {
        try
        {
            _logger.LogInformation("Testing file deletion: {FileId}", fileId);

            var success = await _fileStorageService.DeleteFileAsync(fileId);
            
            return Ok(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing file deletion: {FileId}", fileId);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// List OpenAI files
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<OpenAIFileListResponse>> ListFiles([FromQuery] string? purpose = null)
    {
        try
        {
            _logger.LogInformation("Listing OpenAI files with purpose: {Purpose}", purpose);

            var files = await _openAIFileService.ListFilesAsync(purpose);
            
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get file information
    /// </summary>
    [HttpGet("info/{fileId}")]
    public async Task<ActionResult<OpenAIFile>> GetFileInfo(string fileId)
    {
        try
        {
            _logger.LogInformation("Getting file info: {FileId}", fileId);

            var file = await _openAIFileService.GetFileAsync(fileId);
            
            return Ok(file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info: {FileId}", fileId);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Test large file upload
    /// </summary>
    [HttpPost("upload-large")]
    public async Task<ActionResult<FileUploadResponse>> UploadLargeFile([FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            _logger.LogInformation("Testing large file upload: {FileName}", file.FileName);

            var request = new FileUploadRequest
            {
                FileStream = file.OpenReadStream(),
                Filename = file.FileName,
                Purpose = "assistants",
                ExpiresAfter = new ExpiresAfter
                {
                    Anchor = "created_at",
                    Seconds = 7 * 24 * 60 * 60 // 7 days
                }
            };

            var response = await _openAIFileService.UploadLargeFileAsync(request);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing large file upload");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
