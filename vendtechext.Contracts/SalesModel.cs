using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class ElectricitySaleRequest
    {
        public decimal Amount { get; set; }
        public string MeterNumber { get; set; }
        public string TransactionId { get; set; }
    }

    public class SaleStatusRequest
    {
        public string TransactionId { get; set; }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionUniqueId { get; set; }
        public string VendtechTransactionID { get; set; }
        public bool Finalized { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public decimal? CurrentDealerBalance { get; set; }
        public string MeterNumber { get; set; }
        public int TransactionStatus { get; set; }
        public int PlatFormId { get; set; }
        public string VendStatusDescription { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ReceivedFrom { get; set; }
        public TransactionDto(Transaction x)
        {
            Id = x.Id;
            TransactionUniqueId = x.TransactionUniqueId;
            VendStatusDescription = x.VendStatusDescription;
            Finalized = x.Finalized;
            Amount = x.Amount;
            BalanceBefore = x.BalanceBefore;
            BalanceAfter = x.BalanceAfter;
            CurrentDealerBalance = x.CurrentDealerBalance;
            MeterNumber = x.MeterNumber;
            TransactionStatus = x.TransactionStatus;
        }
    }
}
