using CVAgentApp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CVAgentApp.Infrastructure.Services;

/// <summary>
/// HTTP client service implementation
/// </summary>
public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientService> _logger;

    public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("Making GET request to: {Endpoint}", endpoint);
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddHeaders(request, headers);
        
        var response = await _httpClient.SendAsync(request);
        _logger.LogInformation("GET request completed with status: {StatusCode}", response.StatusCode);
        
        return response;
    }

    public async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("Making POST request to: {Endpoint}", endpoint);
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        AddHeaders(request, headers);
        
        var response = await _httpClient.SendAsync(request);
        _logger.LogInformation("POST request completed with status: {StatusCode}", response.StatusCode);
        
        return response;
    }

    public async Task<HttpResponseMessage> PostAsync(string endpoint, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("Making POST request to: {Endpoint}", endpoint);
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        AddHeaders(request, headers);
        
        var response = await _httpClient.SendAsync(request);
        _logger.LogInformation("POST request completed with status: {StatusCode}", response.StatusCode);
        
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("Making PUT request to: {Endpoint}", endpoint);
        
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        AddHeaders(request, headers);
        
        var response = await _httpClient.SendAsync(request);
        _logger.LogInformation("PUT request completed with status: {StatusCode}", response.StatusCode);
        
        return response;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("Making DELETE request to: {Endpoint}", endpoint);
        
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        AddHeaders(request, headers);
        
        var response = await _httpClient.SendAsync(request);
        _logger.LogInformation("DELETE request completed with status: {StatusCode}", response.StatusCode);
        
        return response;
    }

    public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null) where T : class
    {
        var response = await GetAsync(endpoint, headers);
        return await DeserializeResponse<T>(response);
    }

    public async Task<T> PostAsync<T>(string endpoint, HttpContent content, Dictionary<string, string>? headers = null) where T : class
    {
        var response = await PostAsync(endpoint, content, headers);
        return await DeserializeResponse<T>(response);
    }

    public async Task<T> PostAsync<T>(string endpoint, Dictionary<string, string>? headers = null) where T : class
    {
        var response = await PostAsync(endpoint, headers);
        return await DeserializeResponse<T>(response);
    }

    public async Task<T> PutAsync<T>(string endpoint, HttpContent content, Dictionary<string, string>? headers = null) where T : class
    {
        var response = await PutAsync(endpoint, content, headers);
        return await DeserializeResponse<T>(response);
    }

    public async Task<T> DeleteAsync<T>(string endpoint, Dictionary<string, string>? headers = null) where T : class
    {
        var response = await DeleteAsync(endpoint, headers);
        return await DeserializeResponse<T>(response);
    }

    private void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }
    }

    private async Task<T> DeserializeResponse<T>(HttpResponseMessage response) where T : class
    {
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Response content: {Content}", content);
        
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}


