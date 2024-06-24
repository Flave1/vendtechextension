using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class TransactionDetail
{
    public long TransactionDetailsId { get; set; }

    public long UserId { get; set; }

    public string? TransactionId { get; set; }

    public DateTime RequestDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal? BalanceBefore { get; set; }

    public decimal Amount { get; set; }

    public decimal? CurrentVendorBalance { get; set; }

    public long? MeterId { get; set; }

    public string? MeterNumber1 { get; set; }

    public long? Posid { get; set; }

    public int Status { get; set; }

    public bool IsDeleted { get; set; }

    public int? PlatFormId { get; set; }

    public string? MeterToken1 { get; set; }

    public string? MeterToken2 { get; set; }

    public string? MeterToken3 { get; set; }

    public decimal CurrentDealerBalance { get; set; }

    public string? AccountNumber { get; set; }

    public string? Customer { get; set; }

    public string? RtsuniqueId { get; set; }

    public string? ReceiptNumber { get; set; }

    public decimal TenderedAmount { get; set; }

    public decimal TransactionAmount { get; set; }

    public string? ServiceCharge { get; set; }

    public string? Tariff { get; set; }

    public string TaxCharge { get; set; } = null!;

    public string CostOfUnits { get; set; } = null!;

    public string Units { get; set; } = null!;

    public string DebitRecovery { get; set; } = null!;

    public string? SerialNumber { get; set; }

    public string? CustomerAddress { get; set; }

    public string? Vprovider { get; set; }

    public bool? Finalised { get; set; }

    public int? StatusRequestCount { get; set; }

    public bool? Sold { get; set; }

    public string? VoucherSerialNumber { get; set; }

    public string? VendStatusDescription { get; set; }

    public string? VendStatus { get; set; }

    public string? Request { get; set; }

    public string? Response { get; set; }

    public string? StatusResponse { get; set; }

    public string? DateAndTimeSold { get; set; }

    public string? DateAndTimeFinalised { get; set; }

    public string? DateAndTimeLinked { get; set; }

    public int? QueryStatusCount { get; set; }

    public virtual Platform? PlatForm { get; set; }

    public virtual Po? Pos { get; set; }

    public virtual User User { get; set; } = null!;
}
