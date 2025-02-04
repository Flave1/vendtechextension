using Newtonsoft.Json;
using System.Net;
using System.Text;
using vendtechext.Contracts;
using Xunit;

namespace vendtechext.TEST.Sales
{
    public class ReconcilationTests
    {
        private readonly HttpClient _client;
        public ReconcilationTests()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        public async Task Test_for_successful_response(HttpStatusCode expectedStatusCode)
        {
            //283508,
            // Arrange
            var requestModel = new
            {
                TransactionId = "283458"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/reconcile/v1/start", content);

            // Assert
            //response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            Assert.Equal(expectedStatusCode, response.StatusCode);
            // Additional assertions to validate the response
        }
    }
}
