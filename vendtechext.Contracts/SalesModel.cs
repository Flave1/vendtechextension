using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class ElectricitySaleRequest
    {
        public decimal Amount { get; set; }
        public string MeterNumber { get; set; }
        public string TransactionId { get; set; }
    }

    public class ElectricitySaleRTO
    {
        public decimal Amount { get; set; }
        public string MeterNumber { get; set; }
        public string TransactionId { get; set; }
        public string VendtechTransactionId { get; set; }
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
        public string Date { get; set; }
        public bool IsClaimed { get; set; }
        public string IntegratorName { get; set; }
        public string WalletId { get; set; }
        public TransactionDto(Transaction x)
        {
            Id = x.Id;
            TransactionUniqueId = x.TransactionUniqueId;
            VendStatusDescription = x.VendStatusDescription;
            VendtechTransactionID = x.VendtechTransactionID;
            Finalized = x.Finalized;
            Amount = x.Amount;
            BalanceBefore = x.BalanceBefore;
            BalanceAfter = x.BalanceAfter;
            MeterNumber = x.MeterNumber;
            TransactionStatus = x.TransactionStatus;
            Date = x.CreatedAt.ToString("dd-MM-yyyy hh:mm");
            IsClaimed = x.ClaimedStatus == (int)ClaimedStatus.Claimed;
            IntegratorName = x.Integrator.BusinessName;
            WalletId = x.Integrator.Wallet.WALLET_ID;
        }
    }

    public class TransactionExportDto
    {
        public string Date { get; set; }
        public string Integrator { get; set; }
        public string WalletId { get; set; }
        public string TransactionId { get; set; }
        public string VendtechTransactionID { get; set; }
        public string Status { get; set; }
        public string MeterNumber { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public TransactionExportDto(Transaction x)
        {
            Date = Utils.formatDate(x.CreatedAt);
            Integrator = x.Integrator.BusinessName;
            WalletId = x.Integrator.Wallet.WALLET_ID;
            TransactionId = x.TransactionUniqueId;
            VendtechTransactionID = x.VendtechTransactionID;
            Status = ((TransactionStatus)x.TransactionStatus).ToString();
            MeterNumber = x.MeterNumber;
            BalanceBefore = x.BalanceBefore;
            Amount = x.Amount;
            BalanceAfter = x.BalanceAfter;
        }
    }
}
