using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace signalrserver.Models.DTO
{
    public class TransactionDTO
    {
        [Required]
        [DefaultValue("")]
        public string TransactionId { get; set; }
        public long? MeterId { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal Amount { get; set; }
        [Required]
        [DefaultValue("")]
        public string MeterNumber1 { get; set; }
        [Required]
        [DefaultValue(0)]
        public long? POSId { get; set; }
        [Required]
        [DefaultValue(0)]
        public int Status { get; set; }
        [Required]
        [DefaultValue("")]
        public string CreatedAt { get; set; }
        [Required]
        [DefaultValue("")]
        public string MeterToken1 { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal CurrentDealerBalance { get; set; }
        [Required]
        [DefaultValue("")]
        public string AccountNumber { get; set; }
        [Required]
        [DefaultValue("")]
        public string Customer { get; set; }
        [Required]
        [DefaultValue("")]
        public string RTSUniqueID { get; set; }
        [Required]
        [DefaultValue("")]
        public string ReceiptNumber { get; set; }
        [Required]
        [DefaultValue("")]
        public string ServiceCharge { get; set; }
        [Required]
        [DefaultValue("")]
        public string Tariff { get; set; }
        [Required]
        [DefaultValue("")]
        public string TaxCharge { get; set; }
        [Required]
        [DefaultValue("")]
        public string Units { get; set; }
        [Required]
        [DefaultValue("")]
        public string SerialNumber { get; set; }
        [Required]
        [DefaultValue("")]
        public string CustomerAddress { get; set; }
        [Required]
        [DefaultValue("")]
        public string DebitRecovery { get; set; }
        [Required]
        [DefaultValue("")]
        public string CostOfUnits { get; set; }
        [Required]
        [DefaultValue("")]
        public string VProvider { get; set; }
        [Required]
        [DefaultValue("")]
        public string VoucherSerialNumber { get; set; }
        //[Required]
        //[DefaultValue("")]
        //public string VendStatusDescription { get; set; }
        //[Required]
        //[DefaultValue("")]
        //public string VendStatus { get; set; }
        //[JsonProperty("request")]
        //[Required]
        //[DefaultValue("")]
        //public string Request { get; set; }
        //[Required]
        //[DefaultValue("")]
        //public string Response { get; set; }
        //[Required]
        //[DefaultValue("")]
        //public string StatusResponse { get; set; }
        //[Required]
        //[DefaultValue("")]
        //public string DateAndTimeSold { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal? CurrentVendorBalance { get; set; }
        [Required]
        [DefaultValue(0)]
        public decimal? BalanceBefore { get; set; }


    }

    public class checkTrxDTO
    {
        [Required]
        public string token { get; set; }
    }

    public class checkTrxDTO1
    {
        [Required]
        public string transactionId { get; set; }
    }
}
