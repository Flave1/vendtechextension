using Newtonsoft.Json;
using System.Text;
using System.Net.Http;

namespace vendtechext.BLL.Services
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string uri);
        Task<T> PostAsync<T, Y>(string uri, Y data);
        Task<T> PutAsync<T, Y>(string uri, Y data);
        Task<bool> DeleteAsync(string uri);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("VendTechClient");
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Check if response is successful
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.StatusCode}: {responseContent}");
            }
            
            // Check if response is JSON (not HTML)
            if (responseContent.TrimStart().StartsWith("<"))
            {
                throw new HttpRequestException($"API returned HTML instead of JSON. This usually means the endpoint is not found or there's a server error. Content: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            }
            
            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (JsonReaderException ex)
            {
                throw new HttpRequestException($"Failed to deserialize JSON response: {ex.Message}. Response content: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            }
        }

        public async Task<T> PostAsync<T, Y>(string uri, Y data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(uri, content);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Check if response is successful
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.StatusCode}: {responseContent}");
            }
            
            // Check if response is JSON (not HTML)
            if (responseContent.TrimStart().StartsWith("<"))
            {
                throw new HttpRequestException($"API returned HTML instead of JSON. This usually means the endpoint is not found or there's a server error. Content: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            }
            
            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (JsonReaderException ex)
            {
                throw new HttpRequestException($"Failed to deserialize JSON response: {ex.Message}. Response content: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            }
        }

        public async Task<T> PutAsync<T, Y>(string uri, Y data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(uri, content);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Check if response is successful
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.StatusCode}: {responseContent}");
            }
            
            // Check if response is JSON (not HTML)
            if (responseContent.TrimStart().StartsWith("<"))
            {
                throw new HttpRequestException($"API returned HTML instead of JSON. This usually means the endpoint is not found or there's a server error. Content: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            }
            
            try
            {
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (JsonReaderException ex)
            {
                throw new HttpRequestException($"Failed to deserialize JSON response: {ex.Message}. Response content: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
            }
        }

        public async Task<bool> DeleteAsync(string uri)
        {
            var response = await _httpClient.DeleteAsync(uri);
            return response.IsSuccessStatusCode;
        }
    }

}
