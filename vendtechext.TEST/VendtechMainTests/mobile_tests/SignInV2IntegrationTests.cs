using Newtonsoft.Json;
using System.Text;
using vendtechext.Contracts.VtchMainModels;
using Xunit;

namespace vendtechext.TEST.VendtechMainTests.mobile_tests
{
    public class SignInV2IntegrationTests
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private const string validDevicetoken = "cQxTFwXQDZw:APA91bHOFtrkiIqn7OGwx0ZxiFv8c3NogdH5w8WWOgf8x_t6TJJgyPTJaH7m1XBn0mn_HYHfrS7edMox6Q6xkB0U_2gTAfvQ0VCkXWMQZS6uFWo8r7jcuOIyszQVke7xk5BhJYSyqyxy";
        private const string currentAppVersion = "2.4.8";
        private const string validPasscode = "85236";
        private const string RESET_PASSCODE = "10001";
        private const string PASSCODE_REQUIRED = "The PassCode field is required.";
        private const string ACCOUNT_DISABLED = "YOUR ACCOUNT IS DISABLED! \n PLEASE CONTACT VENDTECH MANAGEMENT";
        public const string INVALID_CREDENTIALS = "INVALID CREDENTIALS \n\n PLEASE RESET YOUR PASSCODE OR \n CONTACT VENDTECH MANAGEMENT";
        public const string POS_NOTFOUND = "POS NOT AVAILABLE! \n PLEASE CONTACT VENDTECH MANAGEMENT";
        public const string LOGIN_SUCCESS = "You have successfully logged in.";
        public const string OUTDATED_APP_VERSION = "APP VERSION IS OUT OF DATE, PLEASE UPDATE APP FROM PLAYSTORE";
        public const string INVALID_PASSCODE = "Invalid Passcode.";
        public SignInV2IntegrationTests()
        {
            _baseUrl = "https://vendtechsl.com/"; // http://localhost:56549/
            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }
        [Theory]
        [InlineData(currentAppVersion, "", validPasscode, RESET_PASSCODE)]//Test empty device token
        [InlineData(currentAppVersion, validDevicetoken, "", PASSCODE_REQUIRED)]//Test Passcode empty
        [InlineData(currentAppVersion, "Invalid_device_token_00012323", validPasscode, INVALID_CREDENTIALS)]//Test wrong Device token
        [InlineData(currentAppVersion, validDevicetoken, "wrong_passcode111", INVALID_PASSCODE)]//Test Wrong Passcode
        [InlineData("2.4.7", validDevicetoken, validPasscode, OUTDATED_APP_VERSION)]//Test Wrong App Version
        [InlineData(currentAppVersion, validDevicetoken, validPasscode, LOGIN_SUCCESS)]//Test Correct App Version
        //[InlineData(currentAppVersion, validDevicetoken, validPasscode, "EXPECTED_MESSAGE")]//Test POS Enabled
        //[InlineData(currentAppVersion, validDevicetoken, validPasscode, "EXPECTED_MESSAGE")]//Test Account Diabled
        //[InlineData(currentAppVersion, validDevicetoken, validPasscode, ACCOUNT_DISABLED)]//Test wrong passcode
        //[InlineData(currentAppVersion, validDevicetoken, validPasscode, "EXPECTED_MESSAGE")]//Test new customer login
        public async Task Post_SignInV2_ReturnsExpectedResult(string appVersion, string deviceToken, string passCode, string expectedResult)
        {

            // Arrange
            var model = new LoginAPIPassCodeModel
            {
                AppVersion = appVersion,
                DeviceToken = deviceToken,
                PassCode = passCode
            };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/Api/Account/SignInV2", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            var apiResponse = JsonConvert.DeserializeObject<MobileAppResponse>(responseString);
            Assert.Equal(expectedResult, apiResponse?.Message);

        }

    }
}
