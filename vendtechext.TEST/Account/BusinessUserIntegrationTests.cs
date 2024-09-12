using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using vendtechext.BLL.Common;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL.Models;
using Xunit;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace vendtechext.TEST.Account
{
    public class AccountControllerIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly Mock<IB2bAccountService> _service;

        public AccountControllerIntegrationTests()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
            _service = new Mock<IB2bAccountService>();
        }

        [Theory]
        [InlineData("John", "Doe", "password123", "12345678", "Test Business", "johndoe@example.com", HttpStatusCode.OK)]
        //[InlineData("John", "Doe", "password123", "1234567890", "Test Business", "johndoe@example.com", HttpStatusCode.BadRequest)]
        public async Task Test_account_Creation(string firstName, string lastName, string password, string phone, string businessName, string email, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestBody = new BusinessUserCommandDTO
            {
                FirstName = firstName,
                LastName = lastName,
                Password = password,
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
                // Setup the mock to expect the method call
                _service.Setup(s => s.DeleteBusinessAccount(email)).Returns(Task.CompletedTask);

                // Call the delete method
                await _service.Object.DeleteBusinessAccount(email);

                // Verify that the delete method was called
                _service.Verify(s => s.DeleteBusinessAccount(email), Times.Once);
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
                Id = Guid.Parse(Id),
                FirstName = firstName,
                LastName = lastName,
                Password = password,
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
    }

}