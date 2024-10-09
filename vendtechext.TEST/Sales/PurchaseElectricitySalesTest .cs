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
        private readonly Mock<IElectricitySalesService> _mockSalesService;
        public PurchaseElectricitySalesTest()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
            _mockSalesService = new Mock<IElectricitySalesService>();
        }

        [Theory]
        [InlineData("GWNeK8vXswba1VPuDiWaWDLBD9zP7nbc96aRDUx1AMYiw6pTLOlU+myx9ujctiu5", 40, "98000142897", "7", HttpStatusCode.OK)]
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
    }
}
