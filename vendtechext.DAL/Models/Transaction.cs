using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models
{
    public class Transaction : AuditTrail
    {
        [Key]
        public Guid Id { get; set; }
        public string TransactionUniqueId { get; set; }
        public string VendtechTransactionID { get; set; }
        public string SellerTransactionID { get; set; }
        public bool Finalized { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string MeterNumber { get; set; }
        public int TransactionStatus { get; set; }
        public int ClaimedStatus { get; set; }
        public bool IsDeleted { get; set; }
        public int PlatFormId { get; set; }
        public string VendStatusDescription { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ReceivedFrom { get; set; }
        public int PaymentStatus { get; set; }
        public Guid IntegratorId { get; set; }
        [ForeignKey("IntegratorId")]
        public virtual Integrator Integrator { get; set; }
    }

}
