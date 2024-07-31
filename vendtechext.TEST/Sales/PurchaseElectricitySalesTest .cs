using Moq;
using System.Net;
using System.Text;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using Xunit;

namespace vendtechext.TEST.Sales
{
    public class PurchaseElectricitySalesTest
    {
        private readonly HttpClient _client;
        private readonly Mock<IRTSSalesService> _mockSalesService;
        public PurchaseElectricitySalesTest()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
            _mockSalesService = new Mock<IRTSSalesService>();
        }

        [Theory]
        [InlineData("wUFj4GiAX/2ka1DYNhRMxw==", "vRtjY3Tns/m4mPdGwcOnpQ==", 40, "98000142897", "123456", HttpStatusCode.OK)]
        public async Task Test_for_successful_response(
            string apiKey, 
            string clientKey, 
            decimal amount,
            string meterNumber,
            string transactionId,
            HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestModel = new RTSRequestmodel
            {
                Auth = new RTSAuthModel
                {
                    Password = apiKey,
                    UserName = clientKey
                },
                Request = "ProcessPrePaidVendingV1",
                Parameters = [
                        new
                        {
                            UserName = apiKey,
                            Password = clientKey,
                            System = "SL"
                        }, "apiV1_VendVoucher", "webapp", "0", "EDSA", $"{amount}", $"{meterNumber}", -1, "ver1.5", transactionId
                       ],
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/edsa/v2/purchase", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var responseString = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<RTSResponse>(responseString);

            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            // Additional assertions to validate the response
        }
    }
}
