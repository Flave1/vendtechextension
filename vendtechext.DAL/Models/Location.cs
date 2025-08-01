using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models
{
    public class Country : AuditTrail
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

     public class City : AuditTrail
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
    }
}
