namespace vendtechext.Contracts
{

    public class MessageBody
    {
        public MessageBody()
        {
            UserId = string.Empty;
            Message = string.Empty;
            InCludeEmail = false;
        }
        public string UserId { get; set; }
        public string Message { get; set; }
        public bool InCludeEmail { get; set; }
    }

    public class APIResponse
    {
        public string status { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string detailed { get; set; } = string.Empty;
        public dynamic result { get; set; }
    }

    public class APIResponse<T>
    {
        public string status { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string detailed { get; set; } = string.Empty;
        public T result { get; set; }
    }
}
