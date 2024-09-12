using System.ComponentModel.DataAnnotations;

namespace vendtechext.DAL.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        public string IntegratorId { get; set; }
        public string TransactionId { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public decimal? CurrentDealerBalance { get; set; }
        public decimal? TenderedAmount { get; set; }
        public string MeterNumber { get; set; }
        public int TransactionStatus { get; set; }
        public int ClaimedStatus { get; set; }
        public bool IsDeleted { get; set; }
        public int PlatFormId { get; set; }
        public string MeterToken1 { get; set; }
        public string MeterToken2 { get; set; }
        public string MeterToken3 { get; set; }
        public string AccountNumber { get; set; }
        public string Customer { get; set; }
        public string RTSUniqueID { get; set; }
        public string ReceiptNumber { get; set; }
        public string ServiceCharge { get; set; }
        public string Tariff { get; set; }
        public string TaxCharge { get; set; }
        public string CostOfUnits { get; set; }
        public string Units { get; set; }
        public string DebitRecovery { get; set; }
        public string SerialNumber { get; set; }
        public string CustomerAddress { get; set; }
        public string VProvider { get; set; }
        public bool Finalised { get; set; }
        public int StatusRequestCount { get; set; }
        public bool Sold { get; set; }
        public string VoucherSerialNumber { get; set; }
        public string VendStatusDescription { get; set; }
        public string VendStatus { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string StatusResponse { get; set; }
        public string DateAndTimeSold { get; set; }
        public string DateAndTimeFinalised { get; set; }
        public string DateAndTimeLinked { get; set; }
        public int QueryStatusCount { get; set; }
    }

}
