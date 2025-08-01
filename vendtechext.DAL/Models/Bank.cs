using System.ComponentModel.DataAnnotations;

namespace vendtechext.DAL.Models
{
    public class Bank : AuditTrail
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
    }

}
