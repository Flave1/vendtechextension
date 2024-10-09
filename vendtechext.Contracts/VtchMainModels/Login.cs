namespace vendtechext.Contracts.VtchMainModels
{
    // Define your LoginAPIPassCodeModel class
    public class LoginAPIPassCodeModel
    {
        public string AppVersion { get; set; }
        public string DeviceToken { get; set; }
        public string PassCode { get; set; }
    }

    // Define your ResponseBase class if needed
    public class ResponseBase
    {
        public string Message { get; set; }
        public string Status { get; set; }
    }

    public class MobileAppResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object TotalCount { get; set; }
    }
}
