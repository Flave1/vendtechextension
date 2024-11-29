using System.ComponentModel.DataAnnotations;

namespace vendtechext.Contracts
{
    public class NotificationDto
    {
        [Key]
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
}
