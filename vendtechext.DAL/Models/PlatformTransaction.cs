using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PlatformTransaction
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int PlatformId { get; set; }

    public decimal Amount { get; set; }

    public decimal AmountPlatform { get; set; }

    public int Status { get; set; }

    public string? Beneficiary { get; set; }

    public string? UserReference { get; set; }

    public int? ApiConnectionId { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long? PosId { get; set; }

    public string? OperatorReference { get; set; }

    public string? PinSerial { get; set; }

    public string? PinNumber { get; set; }

    public string? PinInstructions { get; set; }

    public string? ApiTransactionId { get; set; }

    public long LastPendingCheck { get; set; }

    public long TransactionDetailId { get; set; }

    public virtual PlatformApiConnection? ApiConnection { get; set; }

    public virtual Currency CurrencyNavigation { get; set; } = null!;

    public virtual Platform Platform { get; set; } = null!;

    public virtual Po? Pos { get; set; }

    public virtual User User { get; set; } = null!;
}
