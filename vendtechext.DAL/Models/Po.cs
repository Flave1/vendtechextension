using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Po
{
    public long Posid { get; set; }

    public long? VendorId { get; set; }

    public string? SerialNumber { get; set; }

    public int? VendorType { get; set; }

    public string? Phone { get; set; }

    public bool? Enabled { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal? Balance { get; set; }

    public int? CommissionPercentage { get; set; }

    public bool? EmailNotificationSales { get; set; }

    public bool? EmailNotificationDeposit { get; set; }

    public bool? SmsnotificationSales { get; set; }

    public bool? SmsnotificationDeposit { get; set; }

    public bool IsPassCode { get; set; }

    public string? Email { get; set; }

    public string? PassCode { get; set; }

    public bool? WebSms { get; set; }

    public bool? PosSms { get; set; }

    public bool? PosPrint { get; set; }

    public bool? WebPrint { get; set; }

    public bool? WebBarcode { get; set; }

    public bool? PosBarcode { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsNewPasscode { get; set; }

    public virtual Commission? CommissionPercentageNavigation { get; set; }

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<PendingDeposit> PendingDeposits { get; set; } = new List<PendingDeposit>();

    public virtual ICollection<PlatformTransaction> PlatformTransactions { get; set; } = new List<PlatformTransaction>();

    public virtual ICollection<PosassignedPlatform> PosassignedPlatforms { get; set; } = new List<PosassignedPlatform>();

    public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();

    public virtual User? Vendor { get; set; }
}
