using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PaymentType
{
    public int PaymentTypeId { get; set; }

    public string? Name { get; set; }

    public DateOnly? CreatedAt { get; set; }

    public DateOnly? UpdatedAt { get; set; }

    public bool Active { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<PendingDeposit> PendingDeposits { get; set; } = new List<PendingDeposit>();
}
