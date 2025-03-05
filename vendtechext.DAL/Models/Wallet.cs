using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models
{
    public class Wallet : AuditTrail
    {
        [Key]
        public Guid Id { get; set; }
        public string WALLET_ID { get; set; }
        public decimal Balance { get; set; }
        public decimal BookBalance { get; set; }
        public int CommissionId { get; set; }
        public int MinThreshold { get; set; }
        public bool IsBalanceLowReminderSent { get; set; }
        public int MidnightBalanceAlertSwitch { get; set; }
        public Guid IntegratorId { get; set; }
        [ForeignKey("IntegratorId")]
        public virtual Integrator Integrator { get; set; }
    }
}
