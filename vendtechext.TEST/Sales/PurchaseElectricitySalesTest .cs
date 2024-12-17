using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using Xunit;

namespace vendtechext.TEST.Sales
{
    public class PurchaseElectricitySalesTest
    {
        private readonly HttpClient _client;
        private readonly Mock<IAPISalesService> _mockSalesService;
        const string transactionId = "268017";
        public PurchaseElectricitySalesTest()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
            _mockSalesService = new Mock<IAPISalesService>();
        }

        [Theory]
        [InlineData("FCcHkRm7bBTaJkjgFyL6C2FH6RSGy6ff0YX3zK1kok87R+HL4blEj+PygevBefS0", 40, "98000142897", transactionId, HttpStatusCode.OK)]
        public async Task Test_for_successful_response(
            string apiKey, 
            decimal amount,
            string meterNumber,
            string transactionId,
            HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestModel = new ElectricitySaleRequest
            {
                TransactionId = transactionId,
                MeterNumber = meterNumber,
                Amount = amount,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            content.Headers.Add("X-Api-Key", apiKey);
            // Act
            var response = await _client.PostAsync("/sales/v1/buy", content);

            // Assert
            //response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            APIResponse result = JsonConvert.DeserializeObject<APIResponse>(responseString);

            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            // Additional assertions to validate the response
        }


        [Theory]
        [InlineData("FCcHkRm7bBTaJkjgFyL6C2FH6RSGy6ff0YX3zK1kok87R+HL4blEj+PygevBefS0", transactionId, HttpStatusCode.OK)]
        public async Task Test_for_successful_query(
           string apiKey,
           string transactionId,
           HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestModel = new SaleStatusRequest
            {
                TransactionId = transactionId,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            content.Headers.Add("X-Api-Key", apiKey);
            // Act
            var response = await _client.PostAsync("/sales/v1/status", content);

            // Assert
            //response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            APIResponse result = JsonConvert.DeserializeObject<APIResponse>(responseString);

            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            // Additional assertions to validate the response
        }
    }
}
