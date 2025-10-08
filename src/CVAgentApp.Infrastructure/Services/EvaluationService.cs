using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.DTOs;
using CVAgentApp.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Linq;
using CVAgentApp.Core.Enums;

namespace CVAgentApp.Infrastructure.Services;

/// <summary>
/// Evaluation service implementation for monitoring and improving agent performance
/// </summary>
public class EvaluationService : IEvaluationService
{
    private readonly ILogger<EvaluationService> _logger;

    public EvaluationService(ILogger<EvaluationService> logger)
    {
        _logger = logger;
    }

    public async Task<EvaluationResult> EvaluateWorkflowAsync(Guid sessionId)
    {
        try
        {
            _logger.LogInformation("Evaluating workflow for session: {SessionId}", sessionId);

            var result = new EvaluationResult
            {
                Id = Guid.NewGuid(),
                EvaluationType = "Workflow",
                SessionId = sessionId,
                EvaluatedAt = DateTime.UtcNow
            };

            // Mock evaluation logic - in real implementation, this would analyze the actual workflow
            var score = await CalculateWorkflowScoreAsync(sessionId);
            result.Score = score;
            result.Passed = score >= 0.7; // 70% threshold

            if (!result.Passed)
            {
                result.Issues.Add("Workflow execution time exceeded threshold");
                result.Issues.Add("Some agents failed to complete successfully");
                result.Recommendations.Add("Optimize agent execution time");
                result.Recommendations.Add("Review failed agent logs");
            }

            result.Metrics = new Dictionary<string, object>
            {
                ["executionTime"] = TimeSpan.FromSeconds(30), // Mock
                ["agentsExecuted"] = 5,
                ["successRate"] = 0.8
            };

            _logger.LogInformation("Workflow evaluation completed with score: {Score}", score);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating workflow for session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<EvaluationResult> EvaluateDocumentQualityAsync(string content, DocumentType type)
    {
        try
        {
            _logger.LogInformation("Evaluating document quality for type: {DocumentType}", type);

            var result = new EvaluationResult
            {
                Id = Guid.NewGuid(),
                EvaluationType = "DocumentQuality",
                EvaluatedAt = DateTime.UtcNow
            };

            var score = await CalculateDocumentQualityScoreAsync(content, type);
            result.Score = score;
            result.Passed = score >= 0.8; // 80% threshold for document quality

            // Check for quality issues
            var issues = await IdentifyQualityIssuesAsync(content, type);
            result.Issues.AddRange(issues);

            if (issues.Any())
            {
                result.Recommendations.Add("Improve document formatting");
                result.Recommendations.Add("Add missing sections");
                result.Recommendations.Add("Enhance professional language");
            }

            result.Metrics = new Dictionary<string, object>
            {
                ["contentLength"] = content.Length,
                ["sectionCount"] = CountSections(content),
                ["keywordDensity"] = CalculateKeywordDensity(content),
                ["formattingScore"] = CalculateFormattingScore(content)
            };

            _logger.LogInformation("Document quality evaluation completed with score: {Score}", score);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating document quality");
            throw;
        }
    }

    public async Task<EvaluationResult> EvaluateTruthfulnessAsync(string originalContent, string generatedContent)
    {
        try
        {
            _logger.LogInformation("Evaluating truthfulness of generated content");

            var result = new EvaluationResult
            {
                Id = Guid.NewGuid(),
                EvaluationType = "Truthfulness",
                EvaluatedAt = DateTime.UtcNow
            };

            var score = await CalculateTruthfulnessScoreAsync(originalContent, generatedContent);
            result.Score = score;
            result.Passed = score >= 0.9; // 90% threshold for truthfulness

            // Check for fabricated content
            var fabricatedContent = await DetectFabricatedContentAsync(originalContent, generatedContent);
            if (fabricatedContent.Any())
            {
                result.Issues.Add($"Fabricated content detected: {string.Join(", ", fabricatedContent)}");
                result.Recommendations.Add("Remove fabricated information");
                result.Recommendations.Add("Ensure all content exists in original CV");
            }

            result.Metrics = new Dictionary<string, object>
            {
                ["originalLength"] = originalContent.Length,
                ["generatedLength"] = generatedContent.Length,
                ["fabricatedCount"] = fabricatedContent.Count,
                ["similarityScore"] = CalculateSimilarityScore(originalContent, generatedContent)
            };

            _logger.LogInformation("Truthfulness evaluation completed with score: {Score}", score);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating truthfulness");
            throw;
        }
    }

    public async Task<EvaluationResult> EvaluateATSCompatibilityAsync(string content)
    {
        try
        {
            _logger.LogInformation("Evaluating ATS compatibility");

            var result = new EvaluationResult
            {
                Id = Guid.NewGuid(),
                EvaluationType = "ATSCompatibility",
                EvaluatedAt = DateTime.UtcNow
            };

            var score = await CalculateATSCompatibilityScoreAsync(content);
            result.Score = score;
            result.Passed = score >= 0.75; // 75% threshold for ATS compatibility

            // Check for ATS issues
            var atsIssues = await IdentifyATSIssuesAsync(content);
            result.Issues.AddRange(atsIssues);

            if (atsIssues.Any())
            {
                result.Recommendations.Add("Use simple, clean formatting");
                result.Recommendations.Add("Include relevant keywords");
                result.Recommendations.Add("Ensure proper section headers");
            }

            result.Metrics = new Dictionary<string, object>
            {
                ["keywordDensity"] = CalculateKeywordDensity(content),
                ["sectionHeaders"] = CountSectionHeaders(content),
                ["formattingComplexity"] = CalculateFormattingComplexity(content),
                ["atsFriendlyElements"] = CountATSFriendlyElements(content)
            };

            _logger.LogInformation("ATS compatibility evaluation completed with score: {Score}", score);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating ATS compatibility");
            throw;
        }
    }

    public async Task<List<EvaluationResult>> GetEvaluationHistoryAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting evaluation history from {FromDate} to {ToDate}", fromDate, toDate);

            // Mock implementation - in real scenario, this would query the database
            var results = new List<EvaluationResult>
            {
                new EvaluationResult
                {
                    Id = Guid.NewGuid(),
                    EvaluationType = "Workflow",
                    Score = 0.85,
                    Passed = true,
                    EvaluatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new EvaluationResult
                {
                    Id = Guid.NewGuid(),
                    EvaluationType = "DocumentQuality",
                    Score = 0.92,
                    Passed = true,
                    EvaluatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evaluation history");
            throw;
        }
    }

    public async Task<EvaluationMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting performance metrics from {FromDate} to {ToDate}", fromDate, toDate);

            // Mock implementation - in real scenario, this would calculate from actual data
            var metrics = new EvaluationMetrics
            {
                AverageScore = 0.87,
                SuccessRate = 0.92,
                AverageExecutionTime = 45.5,
                TotalEvaluations = 150,
                PassedEvaluations = 138,
                FailedEvaluations = 12,
                ScoreByType = new Dictionary<string, double>
                {
                    ["Workflow"] = 0.85,
                    ["DocumentQuality"] = 0.92,
                    ["Truthfulness"] = 0.95,
                    ["ATSCompatibility"] = 0.78
                },
                CommonIssues = new List<string>
                {
                    "Low keyword density",
                    "Missing section headers",
                    "Formatting issues"
                },
                TopRecommendations = new List<string>
                {
                    "Improve ATS compatibility",
                    "Enhance document formatting",
                    "Add more relevant keywords"
                }
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            throw;
        }
    }

    private async Task<double> CalculateWorkflowScoreAsync(Guid sessionId)
    {
        // Mock implementation
        await Task.Delay(100);
        return 0.85;
    }

    private async Task<double> CalculateDocumentQualityScoreAsync(string content, DocumentType type)
    {
        // Mock implementation
        await Task.Delay(100);
        return 0.92;
    }

    private async Task<double> CalculateTruthfulnessScoreAsync(string originalContent, string generatedContent)
    {
        // Mock implementation
        await Task.Delay(100);
        return 0.95;
    }

    private async Task<double> CalculateATSCompatibilityScoreAsync(string content)
    {
        // Mock implementation
        await Task.Delay(100);
        return 0.78;
    }

    private async Task<List<string>> IdentifyQualityIssuesAsync(string content, DocumentType type)
    {
        await Task.Delay(50);
        var issues = new List<string>();

        if (content.Length < 200)
        {
            issues.Add("Document too short");
        }

        if (!content.Contains("Experience") && type == DocumentType.CV)
        {
            issues.Add("Missing experience section");
        }

        return issues;
    }

    private async Task<List<string>> DetectFabricatedContentAsync(string originalContent, string generatedContent)
    {
        await Task.Delay(50);
        // Mock implementation - in real scenario, this would use AI to detect fabricated content
        return new List<string>();
    }

    private async Task<List<string>> IdentifyATSIssuesAsync(string content)
    {
        await Task.Delay(50);
        var issues = new List<string>();

        if (content.Contains("table") || content.Contains("column"))
        {
            issues.Add("Contains ATS-unfriendly elements");
        }

        if (CalculateKeywordDensity(content) < 0.02)
        {
            issues.Add("Low keyword density");
        }

        return issues;
    }

    private int CountSections(string content)
    {
        var sectionHeaders = new[] { "Experience", "Education", "Skills", "Projects", "Certifications" };
        return sectionHeaders.Count(header => content.Contains(header, StringComparison.OrdinalIgnoreCase));
    }

    private double CalculateKeywordDensity(string content)
    {
        var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var totalWords = words.Length;

        if (totalWords == 0) return 0;

        var keywords = new[] { "experience", "skills", "education", "certification", "project", "management" };
        var keywordCount = words.Count(word => keywords.Contains(word.ToLower()));

        return (double)keywordCount / totalWords;
    }

    private double CalculateFormattingScore(string content)
    {
        // Mock implementation
        return 0.85;
    }

    private double CalculateSimilarityScore(string originalContent, string generatedContent)
    {
        // Mock implementation
        return 0.92;
    }

    private int CountSectionHeaders(string content)
    {
        var lines = content.Split('\n');
        return lines.Count(line =>
            line.Trim().Length > 0 &&
            line.Trim().Length < 50 &&
            char.IsUpper(line.Trim()[0]) &&
            !line.Contains('.') &&
            !line.Contains(','));
    }

    private double CalculateFormattingComplexity(string content)
    {
        // Mock implementation
        return 0.3;
    }

    private int CountATSFriendlyElements(string content)
    {
        // Mock implementation
        return 8;
    }
}
