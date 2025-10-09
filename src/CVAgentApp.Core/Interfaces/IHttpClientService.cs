namespace CVAgentApp.Core.Interfaces;

/// <summary>
/// Abstract HTTP client service for making API calls
/// </summary>
public interface IHttpClientService
{
    Task<HttpResponseMessage> GetAsync(string endpoint, Dictionary<string, string>? headers = null);
    Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content, Dictionary<string, string>? headers = null);
    Task<HttpResponseMessage> PostAsync(string endpoint, Dictionary<string, string>? headers = null);
    Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent content, Dictionary<string, string>? headers = null);
    Task<HttpResponseMessage> DeleteAsync(string endpoint, Dictionary<string, string>? headers = null);
    Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null) where T : class;
    Task<T> PostAsync<T>(string endpoint, HttpContent content, Dictionary<string, string>? headers = null) where T : class;
    Task<T> PostAsync<T>(string endpoint, Dictionary<string, string>? headers = null) where T : class;
    Task<T> PutAsync<T>(string endpoint, HttpContent content, Dictionary<string, string>? headers = null) where T : class;
    Task<T> DeleteAsync<T>(string endpoint, Dictionary<string, string>? headers = null) where T : class;
}
