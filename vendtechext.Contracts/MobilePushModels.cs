namespace vendtechext.Contracts
{
    public class MessageRequest
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string DeviceToken { get; set; }
        public string NotificationType { get; set; }
        public string Id { get; set; }
    }
    public class MultipleMessageRequest
    {
        public List<MessageRequest> MessageRequests { get; set; }
    }
}
