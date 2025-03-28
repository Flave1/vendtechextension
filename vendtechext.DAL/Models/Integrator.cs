using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models
{
    public class Integrator: AuditTrail
    {
        [Key]
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public string BusinessName { get; set; }
        public string About { get; set; }   
        public string Logo { get; set; }
        public bool Disabled { get; set; } 
        public string ApiKey { get; set; }
        public string SubApiKey { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }
        public virtual Wallet Wallet { get; set; }
    }
}
