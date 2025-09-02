using System.ComponentModel.DataAnnotations;

namespace vendtechext.Contracts
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reciver { get; set; }
        public bool Read { get; set; }
        public int Type { get; set; }
        public string Date { get; set; }
        public string TargetId { get; set; }
    }

    public class NotificationDtoUpdate
    {
        [Key]
        public long Id { get; set; }
    }

    public class NotificationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Type { get; set; } = 0;
        public string TargetId { get; set; } = string.Empty;

        // Optional channel-specific overrides
        public string? SmsMessage { get; set; }
        public string? EmailMessage { get; set; }
        public string? PushMessage { get; set; }

        // Flags to choose channels
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
        public bool SendPush { get; set; }
        public bool SaveToDatabase { get; set; }
    }
}
