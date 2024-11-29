using System.ComponentModel.DataAnnotations;

namespace vendtechext.DAL.Models
{
    public class Notification: AuditTrail
    {
        [Key]
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reciver { get; set; }
        public string Read { get; set; }
        public int Type { get; set; }
        public string TargetId { get; set; }

    }
}
