using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.DAL.DomainBuilders
{
    public class DepositBuilder
    {
        private readonly Deposit _deposit;

        public DepositBuilder()
        {
            _deposit = new Deposit();
        }
        public DepositBuilder(Deposit deposit)
        {
            _deposit = deposit;
        }

        public DepositBuilder SetId(Guid id)
        {
            _deposit.Id = id;
            return this;
        }

        public DepositBuilder SetIntegratorId(Guid integratorId)
        {
            _deposit.IntegratorId = integratorId;
            return this;
        }

        public DepositBuilder SetParentDepositId(Guid? parentDepositId)
        {
            _deposit.CommissionDepositId = parentDepositId;
            return this;
        }

        public DepositBuilder SetPaymentTypeId(int id)
        {
            _deposit.PaymentTypeId = id;
            return this;
        }

        public DepositBuilder SetBalanceBefore(decimal balanceBefore)
        {
            _deposit.BalanceBefore = balanceBefore;
            return this;
        }

        public DepositBuilder SetAmount(decimal amount)
        {
            _deposit.Amount = amount;
            return this;
        }
        public DepositBuilder SetStatus(DepositStatus status)
        {
            _deposit.Status = (int)status;
            return this;
        }

        public DepositBuilder SetBalanceAfter(decimal balanceAfter)
        {
            _deposit.BalanceAfter = balanceAfter;
            return this;
        }

        public DepositBuilder SetReference(string reference)
        {
            _deposit.Reference = reference;
            return this;
        }

        public DepositBuilder SetTransactionId(string transactionId)
        {
            _deposit.TransactionId = transactionId;
            return this;
        }

        public DepositBuilder SetCreatedAt(DateTime createdAt)
        {
            _deposit.CreatedAt = createdAt;  // Assuming AuditTrail has a CreatedAt field.
            return this;
        }

        public DepositBuilder SetUpdatedAt(DateTime updatedAt)
        {
            _deposit.UpdatedAt = updatedAt;  // Assuming AuditTrail has an UpdatedAt field.
            return this;
        }

        public Deposit Build()
        {
            return _deposit;
        }
    }
}
