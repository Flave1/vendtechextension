using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using vendtechext.BLL.Common;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using Xunit;

namespace vendtechext.TEST.Account
{
    public class AccountControllerIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly Mock<IIntegratorService> _service;

        public AccountControllerIntegrationTests()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
            _service = new Mock<IIntegratorService>();
        }

        [Theory]
        [InlineData("Victor", "Doe", "password123", "12345678", "VENDETCHSL", "vendtechsl@example.com", HttpStatusCode.OK)]
        //[InlineData("John", "Doe", "password123", "1234567890", "Test Business", "johndoe@example.com", HttpStatusCode.BadRequest)]
        public async Task Test_account_Creation(string firstName, string lastName, string password, string phone, string businessName, string email, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestBody = new BusinessUserCommandDTO
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                BusinessName = businessName,
                Email = email
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/account/v1/create-account", content);
            string apiResponseString = await response.Content.ReadAsStringAsync();
            APIResponse responseObject = JsonConvert.DeserializeObject<APIResponse>(apiResponseString);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
            if(expectedStatusCode == HttpStatusCode.OK && response.StatusCode == expectedStatusCode)
            {
                //// Setup the mock to expect the method call
                //_service.Setup(s => s.DeleteBusinessAccount(email)).Returns(Task.CompletedTask);

                //// Call the delete method
                //await _service.Object.DeleteBusinessAccount(email);

                //// Verify that the delete method was called
                //_service.Verify(s => s.DeleteBusinessAccount(email), Times.Once);
            }
        }

        [Theory]
        [InlineData("", "John", "Doe", "password123", "1234567890", "Test Business One", "johndoe@example.com", HttpStatusCode.OK)]
        [InlineData("", "John", "Doe", "password123", "1234567890", "Test Business", "johndoe@example.com", HttpStatusCode.BadRequest)] //Already Exist
        public async Task Test_account_update(string Id, string firstName, string lastName, string password, string phone, string businessName, string email, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestBody = new BusinessUserCommandDTO
            {
                //Id = Guid.Parse(Id),
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                BusinessName = businessName,
                Email = email
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/account/v1/update-account", content);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);

        }
        [Theory]
        [InlineData("76601994", "AFRO INTERNATIONAL LTD", "navin@afroint.com")]
        public void Test_api_key_generattion(string phone, string businessName, string email)
        {
            var apiKey = AesEncryption.Encrypt(businessName + email + phone);
            var derypted = AesEncryption.Decrypt(businessName + email + phone);

            Assert.NotEqual(derypted, apiKey);

        }
        
    }

}