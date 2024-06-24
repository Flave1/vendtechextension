using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class DepositLog
{
    public long DepositLogId { get; set; }

    public long UserId { get; set; }

    public long DepositId { get; set; }

    public int PreviousStatus { get; set; }

    public int NewStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Deposit Deposit { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
