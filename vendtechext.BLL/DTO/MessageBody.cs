namespace vendtechext.BLL.DTO
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
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Detailed { get; set; }
        public object Result { get; set; }
    }
}
