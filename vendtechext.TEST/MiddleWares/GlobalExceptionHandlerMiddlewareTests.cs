using System.Net;
using Xunit;

namespace vendtechext.TEST.MiddleWares
{
    public class GlobalExceptionHandlerMiddlewareTests
    {
        private readonly HttpClient _client;

        public GlobalExceptionHandlerMiddlewareTests()
        {
            TestServerFixture ts = new TestServerFixture();
            _client = ts.Client;
            _client.DefaultRequestHeaders.Add("X-Api-Key", "wUFj4GiAX/2ka1DYNhRMxw==");
            _client.DefaultRequestHeaders.Add("X-Client", "vRtjY3Tns/m4mPdGwcOnpQ==");
        }

        [Fact]
        public async Task GlobalExceptionHandlerMiddleware_Returns_InternalServerError_On_Exception()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/auth/v2/user/exception");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("failed", content);
        }
    }
}
