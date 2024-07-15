using Newtonsoft.Json;
using System.Text;
using vendtechext.BLL.Interfaces;

namespace vendtechext.BLL.Services
{
    public class HttpRequestService : IHttpRequestService
    {
        private const int MaxRetryAttempts = 3;
        private readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

        public async Task<HttpResponseMessage> SendPostAsync<T>(string requestUrl, T body = default)
        {
            int attempt = 0;

            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    var jsBody = JsonConvert.SerializeObject(body);
                    using (var httpClient = new HttpClient())
                    {
                        var request = new HttpRequestMessage
                        {
                            RequestUri = new Uri(requestUrl),
                            Method = HttpMethod.Post,
                            Content = !string.IsNullOrEmpty(jsBody) ? new StringContent(jsBody, Encoding.UTF8, "application/json") : null
                        };
                        return await httpClient.SendAsync(request);
                    }
                }
                catch (HttpRequestException)
                {
                    attempt++;
                    if (attempt >= MaxRetryAttempts)
                    {
                        throw;
                    }
                    await Task.Delay(RetryDelay);
                }
            }

            // This line should never be reached
            throw new InvalidOperationException("Unexpected error in SendPostAsync.");
        }

         
    }
}
