namespace vendtechext.Contracts
{

    public class MessageBody
    {
        public MessageBody()
        {
            UserId = string.Empty;
            Message = string.Empty;
        }
        public string UserId { get; set; }
        public string Message { get; set; }
    }

    public class APIResponse
    {
        public string status { get; set; } = string.Empty;
        public int statusCode { get; set; } = 200;
        public string message { get; set; } = string.Empty;
        public string detailed { get; set; } = string.Empty;
        public object result { get; set; }
    }
}
