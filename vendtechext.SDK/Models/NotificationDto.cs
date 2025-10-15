using System.ComponentModel.DataAnnotations;

namespace vendtechext.SDK.Models
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
        public string Subject { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;

        // Optional channel-specific overrides
        public string SmsMessage { get; set; }
        public string EmailMessage { get; set; }
        public string PushMessage { get; set; }

        // Flags to choose channels
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
        public bool SendPush { get; set; }
        public bool SaveToDatabase { get; set; }
        public bool ToAdmin { get; set; } = false;
        public EmailTypeEnum Emailtype { get; set; } = 0;
        public NotificationType NotificationType { get; set; }
        public NotificationEvent NotificationEvent { get; set; } = 0;


        // Navigation properties (these would need to be set from the calling code)
        public string FirstName { get; set; }
        public string DeviceToken { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
    }
    public record NotificationResponse(bool Success, string Message);

    public class SMSRequest
    {
        public SMSRequest()
        {
            Authorization = "dnRlY2g6cFhQcnkkR3BuXzVVdndfIQ==";
            Sender = "VENDTECH";
        }
        public string Authorization { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Payload { get; set; }
    }
}
