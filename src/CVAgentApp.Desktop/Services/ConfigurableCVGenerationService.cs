using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using CVAgentApp.Infrastructure.Services;

namespace CVAgentApp.Desktop.Services;

public class ConfigurableCVGenerationService : ICVGenerationService
{
    private readonly ILogger<ConfigurableCVGenerationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ICVGenerationService _realService;
    private readonly ICVGenerationService _mockService;

    public ConfigurableCVGenerationService(
        ILogger<ConfigurableCVGenerationService> logger,
        IConfiguration configuration,
        CVGenerationService realService,
        MockCVGenerationService mockService)
    {
        _logger = logger;
        _configuration = configuration;
        _realService = realService;
        _mockService = mockService;
    }

    public async Task<CVGenerationResponse> GenerateCVAsync(CVGenerationRequest request)
    {
        try
        {
            // Check if OpenAI API key is configured
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your-openai-api-key-here")
            {
                _logger.LogWarning("OpenAI API key not configured, using mock service for demonstration");
                return await _mockService.GenerateCVAsync(request);
            }

            _logger.LogInformation("Using real CV generation service with OpenAI integration");
            return await _realService.GenerateCVAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in real CV generation service, falling back to mock service");
            return await _mockService.GenerateCVAsync(request);
        }
    }

    public async Task<SessionStatusResponse> GetSessionStatusAsync(string sessionToken)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your-openai-api-key-here")
            {
                return await _mockService.GetSessionStatusAsync(sessionToken);
            }

            return await _realService.GetSessionStatusAsync(sessionToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in real session status service, falling back to mock service");
            return await _mockService.GetSessionStatusAsync(sessionToken);
        }
    }

    public async Task<byte[]> DownloadDocumentAsync(Guid documentId)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your-openai-api-key-here")
            {
                return await _mockService.DownloadDocumentAsync(documentId);
            }

            return await _realService.DownloadDocumentAsync(documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in real document download service, falling back to mock service");
            return await _mockService.DownloadDocumentAsync(documentId);
        }
    }

    public async Task<bool> DeleteSessionAsync(string sessionToken)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your-openai-api-key-here")
            {
                return await _mockService.DeleteSessionAsync(sessionToken);
            }

            return await _realService.DeleteSessionAsync(sessionToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in real session deletion service, falling back to mock service");
            return await _mockService.DeleteSessionAsync(sessionToken);
        }
    }

    public async Task<CandidateAnalysisResponse> AnalyzeCandidateAsync(IFormFile cvFile)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your-openai-api-key-here")
            {
                return await _mockService.AnalyzeCandidateAsync(cvFile);
            }

            return await _realService.AnalyzeCandidateAsync(cvFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in real candidate analysis service, falling back to mock service");
            return await _mockService.AnalyzeCandidateAsync(cvFile);
        }
    }

    public async Task<JobAnalysisResponse> AnalyzeJobPostingAsync(string jobUrl, string? companyName = null)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your-openai-api-key-here")
            {
                return await _mockService.AnalyzeJobPostingAsync(jobUrl, companyName);
            }

            return await _realService.AnalyzeJobPostingAsync(jobUrl, companyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in real job analysis service, falling back to mock service");
            return await _mockService.AnalyzeJobPostingAsync(jobUrl, companyName);
        }
    }
}
