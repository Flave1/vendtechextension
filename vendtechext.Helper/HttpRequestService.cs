using Azure.Core;
using Newtonsoft.Json;
using System.Text;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.Helper
{
    public class HttpRequestService
    {
        private const int MaxRetryAttempts = 3;
        private readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);
        private readonly ILogService _log;

        public HttpRequestService(ILogService log)
        {
            _log = log;
        }

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
                    _log.Log(LogType.Error, $"HttpRequestException retries {attempt}");
                    if (attempt >= MaxRetryAttempts)
                    {
                        throw;
                    }
                    await Task.Delay(RetryDelay);
                }
            }

            // This line should never be reached
            _log.Log(LogType.Error, $"SendPostAsync InvalidOperationException retries {attempt}");
            throw new InvalidOperationException("Unexpected error in SendPostAsync.");
        }


    }
}
