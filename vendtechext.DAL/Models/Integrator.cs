using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models
{
    public class Integrator: AuditTrail
    {
        [Key]
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string About { get; set; }
        public string ApiKey { get; set; }
        public Guid IntegratorId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }
    }
}
