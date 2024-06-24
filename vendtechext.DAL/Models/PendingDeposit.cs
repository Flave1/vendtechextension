using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PendingDeposit
{
    public long PendingDepositId { get; set; }

    public long UserId { get; set; }

    public long Posid { get; set; }

    public string? TransactionId { get; set; }

    public int PaymentType { get; set; }

    public string CheckNumberOrSlipId { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal? PercentageAmount { get; set; }

    public string? Comments { get; set; }

    public int Status { get; set; }

    public string? ChequeBankName { get; set; }

    public string? NameOnCheque { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int PendingBankAccountId { get; set; }

    public decimal? NewBalance { get; set; }

    public bool IsAudit { get; set; }

    public string? ValueDate { get; set; }

    public DateTime? NextReminderDate { get; set; }

    public bool IsDeleted { get; set; }

    public long ApprovedDepId { get; set; }

    public DateTime? ValueDateStamp { get; set; }

    public virtual PaymentType PaymentTypeNavigation { get; set; } = null!;

    public virtual Po Pos { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
