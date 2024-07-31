using System.Text;
using Xunit;

namespace vendtechext.TEST.Sales
{
    public class RTSSalesControllerTest
    {
        private readonly HttpClient _client;

        public RTSSalesControllerTest()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
        }
        [Fact]
        public async Task InvokeAsync_ValidJson_SuccessfulResponse()
        {
            // Arrange
            var validJson = @"{ ""Auth"": { ""UserName"": ""username"", ""Password"": ""password"" }, ""Request"": ""ProcessPrePaidVendingV1"", ""Parameters"": [ ""param1"", ""param2"" ] }";
            var content = new StringContent(validJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/edsa/v2/purchase/json", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("username", responseString);
        }

        [Fact]
        public async Task InvokeAsync_InvalidJson_ReturnsBadRequest()
        {
            // Arrange
            var invalidJson = @"{ ""InvalidJson"": ";
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/edsa/v2/purchase", content);

            // Assert
            Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, (int)response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_NullRequestBody_ReturnsBadRequest()
        {
            // Arrange 
            var content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/edsa/v2/purchase", content);

            // Assert
            Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, (int)response.StatusCode);
        }
    }
}
