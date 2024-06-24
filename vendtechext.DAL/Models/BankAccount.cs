using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class BankAccount
{
    public int BankAccountId { get; set; }

    public string? BankName { get; set; }

    public string? AccountName { get; set; }

    public string? AccountNumber { get; set; }

    public string? Bban { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
}
