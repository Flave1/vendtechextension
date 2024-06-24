using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Deposit
{
    public long DepositId { get; set; }

    public long UserId { get; set; }

    public long Posid { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? TransactionId { get; set; }

    public int PaymentType { get; set; }

    public decimal? BalanceBefore { get; set; }

    public decimal Amount { get; set; }

    public decimal? PercentageAmount { get; set; }

    public decimal? NewBalance { get; set; }

    public decimal? AgencyCommission { get; set; }

    public string CheckNumberOrSlipId { get; set; } = null!;

    public string? Comments { get; set; }

    public int Status { get; set; }

    public string? ChequeBankName { get; set; }

    public string? NameOnCheque { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int BankAccountId { get; set; }

    public bool IsAudit { get; set; }

    public string? ValueDate { get; set; }

    public DateTime? NextReminderDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? ValueDateStamp { get; set; }

    public virtual BankAccount BankAccount { get; set; } = null!;

    public virtual ICollection<DepositLog> DepositLogs { get; set; } = new List<DepositLog>();

    public virtual PaymentType PaymentTypeNavigation { get; set; } = null!;

    public virtual Po Pos { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
