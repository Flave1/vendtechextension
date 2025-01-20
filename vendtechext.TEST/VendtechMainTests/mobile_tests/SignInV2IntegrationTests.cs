using Newtonsoft.Json;
using System;
using System.Text;
using vendtechext.Contracts.VtchMainModels;
using Xunit;

namespace vendtechext.TEST.VendtechMainTests.mobile_tests
{
    public class SignInV2IntegrationTests
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private const string validDevicetoken = "eRrpCs0h19I:APA91bFX2OFairg2T8-krPorzi1VrX_vSIAihwIEEz5fdDr381kXxHWTUKcEr4Q9sOCo8x3Pnt2EZ0u-FZH63KhqLn-iZeMVE509GtT_q_5U85bnotZh15c";
        private const string currentAppVersion = "2.5.4";
        private const string validPasscode = "73086";
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
            _baseUrl = "http://localhost:56549/"; // https://vendtechsl.com/
            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }
        [Theory]
        //[InlineData(currentAppVersion, "", validPasscode, RESET_PASSCODE)]//Test empty device token
        //[InlineData(currentAppVersion, validDevicetoken, "", PASSCODE_REQUIRED)]//Test Passcode empty
        //[InlineData(currentAppVersion, "Invalid_device_token_00012323", validPasscode, INVALID_CREDENTIALS)]//Test wrong Device token
        //[InlineData(currentAppVersion, validDevicetoken, "wrong_passcode111", INVALID_PASSCODE)]//Test Wrong Passcode
        //[InlineData("2.4.7", validDevicetoken, validPasscode, OUTDATED_APP_VERSION)]//Test Wrong App Version
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


        [Theory]
        [InlineData("vblell@gmail.com")]
        public async Task Post_ForgotPasscode2(string email)
        {

            // Arrange
            var model = new 
            {
                email,
            };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/Api/Account/ForgotPasscode2", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            var apiResponse = JsonConvert.DeserializeObject<MobileAppResponse>(responseString);
            //Assert.Equal(expectedResult, apiResponse?.Message);

        }

        [Theory]
        [InlineData("3313")]
        public async Task Post_VerifyAccountVerificationCode(string code)
        {

            // Arrange
            var model = new
            {
                code,
                userId = 40251
            };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/Api/Account/VerifyAccountVerificationCode", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        }

        [Theory]
        [InlineData(validPasscode, validDevicetoken)]
        public async Task Post_SignInNewpasscode(string code, string token)
        {

            // Arrange
            var model = new
            {
                UserId = 40251,
                PassCode = code,
                DeviceToken = token,
                AppType = "Mobile",
                AppVersion = "2.5.4"
            };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/Api/Account/SignInNewpasscode", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        }

    }
}
