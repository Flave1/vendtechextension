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
        public string status { get; set; }
        public int statusCode { get; set; }
        public string message { get; set; }
        public string detailed { get; set; }
        public object result { get; set; }
    }
}
