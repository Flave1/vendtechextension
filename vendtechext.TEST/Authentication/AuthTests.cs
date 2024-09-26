using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace vendtechext.TEST.Authentication
{
    public class AuthenticationControllerIntegrationTests
    {
        private readonly HttpClient _client;

        public AuthenticationControllerIntegrationTests( )
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
        }

        [Theory]
        [InlineData("wUFj4GiAX/2ka1DYNhRMxw==", "vRtjY3Tns/m4mPdGwcOnpQ==", HttpStatusCode.OK)]
        [InlineData("invalid_api_key", "vRtjY3Tns/m4mPdGwcOnpQ==", HttpStatusCode.Unauthorized)]
        [InlineData("wUFj4GiAX/2ka1DYNhRMxw==", "invalid_client", HttpStatusCode.Unauthorized)]
        [InlineData("", "vRtjY3Tns/m4mPdGwcOnpQ==", HttpStatusCode.Unauthorized)]
        [InlineData("wUFj4GiAX/2ka1DYNhRMxw==", "", HttpStatusCode.Unauthorized)]
        public async Task Validate_Endpoint_Returns_Correct_Status_Code(string apiKey, string clientKey, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestBody = new { UserId = "test_user_id" };
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            // Set headers
            _client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            _client.DefaultRequestHeaders.Add("X-Client", clientKey);

            // Act
            var response = await _client.PostAsync("/auth/v2/user/validate", content);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }

}