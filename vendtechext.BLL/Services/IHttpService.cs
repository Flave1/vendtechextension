using Newtonsoft.Json;
using System.Text;

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
            //response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<T> PostAsync<T, Y>(string uri, Y data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(uri, content);
            //response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<T> PutAsync<T, Y>(string uri, Y data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(uri, content);
            //response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<bool> DeleteAsync(string uri)
        {
            var response = await _httpClient.DeleteAsync(uri);
            return response.IsSuccessStatusCode;
        }
    }

}
