using Microsoft.EntityFrameworkCore;
using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class DepositDto
    {
        public Guid Id { get; set; }
        public Guid IntegratorId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Reference { get; set; }
        public string TransactionId { get; set; }
        public string PaymentTypeName { get; set; }
        public string WalletId { get; set; }
        public string IntegratorName { get; set; }
        public string Date { get; set; }
  
        public DepositDto(Deposit d)
        {
            WalletId = d.Integrator.Wallet.WALLET_ID;
            Reference = d.Reference;
            BalanceBefore = d.BalanceBefore;
            Amount = d.Amount;
            BalanceAfter = d.BalanceAfter;
            IntegratorId = d.IntegratorId;
            TransactionId = d.TransactionId;
            Id = d.Id;
            IntegratorName = d.Integrator.BusinessName;
            PaymentTypeName = d.PaymentMethod.Name;
            Date = d.CreatedAt.ToString("dd-MM-yyyy hh:mm");
        }
    }

    public class DepositExcelDto
    {
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Reference { get; set; }
        public string TransactionId { get; set; }
        public string PaymentTypeName { get; set; }
        public string WalletId { get; set; }
        public string IntegratorName { get; set; }
        public string Date { get; set; }
        public DepositExcelDto(Deposit d)
        {
            WalletId = d.Integrator.Wallet.WALLET_ID;
            Reference = d.Reference;
            BalanceBefore = d.BalanceBefore;
            Amount = d.Amount;
            BalanceAfter = d.BalanceAfter;
            TransactionId = d.TransactionId;
            IntegratorName = d.Integrator.BusinessName;
            PaymentTypeName = d.PaymentMethod.Name;
            Date = d.CreatedAt.ToString("dd-MM-yyyy hh:mm");
        }
    }

    public class CreateDepositDto
    {
        public Guid IntegratorId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Reference { get; set; }
        public int PaymentTypeId { get; set; }
    }

    public class DepositRequest
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public int PaymentTypeId { get; set; }
    }
    public class ApproveDepositRequest
    {
        public bool Approve { get; set; }
        public Guid DepositId { get; set; }
        public Guid IntegratorId { get; set; }
        public string ApprovingUserId;
    }

    public class PaymentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class WalletDTO
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Logo { get; set; }
        public decimal BookBalance { get; set; }
        public decimal WalletBalance { get; set; }
        public List<LastDeposit> LastDeposit { get; set; }
    }

    public class LastDeposit
    {
        public string Date { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public int Status { get; set; }
    }

    public class TodaysTransaction
    {
        public decimal Sales { get; set;}
        public decimal Deposits { get; set; }
    }
}
