using Microsoft.Extensions.Http;
using Newtonsoft.Json;
using System.Text;
using vendtechext.DAL.Common;
using System.Net.Http;

namespace vendtechext.Helper
{
    public class HttpRequestService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly LogService _log;
        private const int MaxRetryAttempts = 3;
        private readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

        public HttpRequestService(LogService log, IHttpClientFactory clientFactory)
        {
            _log = log;
            _clientFactory = clientFactory;
        }

        public async Task<HttpResponseMessage> SendPostAsync<T>(string requestUrl, T body = default)
        {
            int attempt = 0;

            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    var client = _clientFactory.CreateClient("VendTech");
                    var jsBody = JsonConvert.SerializeObject(body);
                    var request = new HttpRequestMessage
                    {
                        RequestUri = new Uri(requestUrl),
                        Method = HttpMethod.Post,
                        Content = !string.IsNullOrEmpty(jsBody) 
                            ? new StringContent(jsBody, Encoding.UTF8, "application/json") 
                            : null
                    };

                    return await client.SendAsync(request);
                }
                catch (HttpRequestException ex)
                {
                    attempt++;
                    _log.Log(LogType.Error, $"HttpRequestException retries {attempt}", ex);
                    
                    if (attempt >= MaxRetryAttempts)
                    {
                        throw;
                    }
                    await Task.Delay(RetryDelay);
                }
            }

            _log.Log(LogType.Error, $"SendPostAsync InvalidOperationException retries {attempt}");
            throw new InvalidOperationException("Unexpected error in SendPostAsync.");
        }
    }
}
