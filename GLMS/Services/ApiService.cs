using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GLMS.Web.Services
{
    public interface IApiService
    {
        Task<string?> LoginAsync(string email, string password);
        Task<T?> GetAsync<T>(string endpoint, string token);
        Task<T?> PostAsync<T>(string endpoint, object body, string token);
        Task<T?> PatchAsync<T>(string endpoint, object body, string token);
        Task<bool> PutAsync(string endpoint, object body, string token);
        Task<bool> DeleteAsync(string endpoint, string token);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]!);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var body = new { email, password };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("token").GetString();
        }

        public async Task<T?> GetAsync<T>(string endpoint, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object body, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object body, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task<bool> PutAsync(string endpoint, object body, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string endpoint, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}