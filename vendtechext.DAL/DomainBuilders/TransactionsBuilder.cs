using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.DAL.DomainBuilders
{
    public class TransactionsBuilder
    {
        private readonly Transaction _transaction;

        public TransactionsBuilder()
        {
            _transaction = new Transaction();
        }
        public TransactionsBuilder(Transaction transaction)
        {
            _transaction = transaction;
        }

        public TransactionsBuilder WithId(Guid id)
        {
            _transaction.Id = id;
            return this;
        }

        public TransactionsBuilder WithIntegratorId(Guid integratorId)
        {
            _transaction.IntegratorId = integratorId;
            return this;
        }
        public TransactionsBuilder WithReceivedFrom(string receivedFrom)
        {
            _transaction.ReceivedFrom = receivedFrom;
            return this;
        }

        public TransactionsBuilder WithTransactionId(string transactionId)
        {
            _transaction.VendtechTransactionID = transactionId;
            return this;
        }
        public TransactionsBuilder WithTransactionUniqueId(string transactionUniqueId)
        {
            _transaction.TransactionUniqueId = transactionUniqueId;
            return this;
        }

        public TransactionsBuilder WithCreatedAt(DateTime createdAt)
        {
            _transaction.CreatedAt = createdAt;
            return this;
        }

        public TransactionsBuilder WithBalanceBefore(decimal balanceBefore)
        {
            _transaction.BalanceBefore = balanceBefore;
            return this;
        }

        public TransactionsBuilder WithAmount(decimal amount)
        {
            _transaction.Amount = amount;
            return this;
        }

        public TransactionsBuilder WithBalanceAfter(decimal balanceAfter)
        {
            _transaction.BalanceAfter = balanceAfter;
            return this;
        }

        public TransactionsBuilder WithCurrentDealerBalance(decimal? currentDealerBalance)
        {
            _transaction.CurrentDealerBalance = currentDealerBalance;
            return this;
        }

        public TransactionsBuilder WithMeterNumber(string meterNumber)
        {
            _transaction.MeterNumber = meterNumber;
            return this;
        }

        public TransactionsBuilder WithTransactionStatus(TransactionStatus transactionStatus)
        {
            _transaction.TransactionStatus = (int)transactionStatus;
            return this;
        }

        public TransactionsBuilder WithClaimedStatus(int claimedStatus)
        {
            _transaction.ClaimedStatus = claimedStatus;
            return this;
        }

        public TransactionsBuilder WithIsDeleted(bool isDeleted)
        {
            _transaction.IsDeleted = isDeleted;
            return this;
        }

        public TransactionsBuilder WithPlatFormId(int platFormId)
        {
            _transaction.PlatFormId = platFormId;
            return this;
        }

      
        public TransactionsBuilder WithVendStatusDescription(string vendStatusDescription)
        {
            _transaction.VendStatusDescription = vendStatusDescription;
            return this;
        }


        public TransactionsBuilder WithRequest(string request)
        {
            _transaction.Request = request;
            return this;
        }

        public TransactionsBuilder WithResponse(string response)
        {
            _transaction.Response = response;
            return this;
        }

        public TransactionsBuilder WithFinalized(bool finalized = false)
        {
            _transaction.Finalized = finalized;
            return this;
        }


        public Transaction Build()
        {
            return _transaction;
        }
    }

}
