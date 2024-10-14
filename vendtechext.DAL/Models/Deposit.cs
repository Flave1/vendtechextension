using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models
{
    public class Deposit : AuditTrail
    {
        [Key]
        public Guid Id { get; set; }
        public int Status { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Reference { get; set; }
        public string TransactionId { get; set; }
        public int PaymentTypeId { get; set; }
        public Guid IntegratorId { get; set; }
        [ForeignKey("IntegratorId")]
        public virtual Integrator Integrator { get; set; }

    }
}
