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
        public Guid DepositId { get; set; }
        public Guid IntegratorId { get; set; }
    }

    public class PaymentType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class WalletDTO
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BookBalance { get; set; }
        public decimal WalletBalance { get; set; }
        public List<LastDeposit> LastDeposit { get; set; }
    }

    public class LastDeposit
    {
        public string Date { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
    }
}
