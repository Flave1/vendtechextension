using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models
{
    public class RolePermission: AuditTrail
    {
        [Key]
        public int Id { get; set; }
        public string RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public virtual AppRole Role { get; set; }
        public string PermissionIds { get; set; }
    }
}
