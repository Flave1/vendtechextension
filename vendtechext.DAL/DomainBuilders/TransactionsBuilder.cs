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

        public TransactionsBuilder WithIntegratorId(string integratorId)
        {
            _transaction.IntegratorId = integratorId;
            return this;
        }

        public TransactionsBuilder WithTransactionId(string transactionId)
        {
            _transaction.TransactionId = transactionId;
            return this;
        }

        public TransactionsBuilder WithRequestDate(DateTime requestDate)
        {
            _transaction.RequestDate = requestDate;
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

        public TransactionsBuilder WithTenderedAmount(decimal? tenderedAmount)
        {
            _transaction.TenderedAmount = tenderedAmount;
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

        public TransactionsBuilder WithMeterToken1(string meterToken1)
        {
            _transaction.MeterToken1 = meterToken1;
            return this;
        }

        public TransactionsBuilder WithMeterToken2(string meterToken2)
        {
            _transaction.MeterToken2 = meterToken2;
            return this;
        }

        public TransactionsBuilder WithMeterToken3(string meterToken3)
        {
            _transaction.MeterToken3 = meterToken3;
            return this;
        }

        public TransactionsBuilder WithAccountNumber(string accountNumber)
        {
            _transaction.AccountNumber = accountNumber;
            return this;
        }

        public TransactionsBuilder WithCustomer(string customer)
        {
            _transaction.Customer = customer;
            return this;
        }

        public TransactionsBuilder WithRTSUniqueID(string rtsUniqueId)
        {
            _transaction.RTSUniqueID = rtsUniqueId;
            return this;
        }

        public TransactionsBuilder WithReceiptNumber(string receiptNumber)
        {
            _transaction.ReceiptNumber = receiptNumber;
            return this;
        }

        public TransactionsBuilder WithServiceCharge(string serviceCharge)
        {
            _transaction.ServiceCharge = serviceCharge;
            return this;
        }

        public TransactionsBuilder WithTariff(string tariff)
        {
            _transaction.Tariff = tariff;
            return this;
        }

        public TransactionsBuilder WithTaxCharge(string taxCharge)
        {
            _transaction.TaxCharge = taxCharge;
            return this;
        }

        public TransactionsBuilder WithCostOfUnits(string costOfUnits)
        {
            _transaction.CostOfUnits = costOfUnits;
            return this;
        }

        public TransactionsBuilder WithUnits(string units)
        {
            _transaction.Units = units;
            return this;
        }

        public TransactionsBuilder WithDebitRecovery(string debitRecovery)
        {
            _transaction.DebitRecovery = debitRecovery;
            return this;
        }

        public TransactionsBuilder WithSerialNumber(string serialNumber)
        {
            _transaction.SerialNumber = serialNumber;
            return this;
        }

        public TransactionsBuilder WithCustomerAddress(string customerAddress)
        {
            _transaction.CustomerAddress = customerAddress;
            return this;
        }

        public TransactionsBuilder WithVProvider(string vProvider)
        {
            _transaction.VProvider = vProvider;
            return this;
        }

        public TransactionsBuilder WithFinalised(bool finalised)
        {
            _transaction.Finalised = finalised;
            return this;
        }

        public TransactionsBuilder WithStatusRequestCount(int statusRequestCount)
        {
            _transaction.StatusRequestCount = statusRequestCount;
            return this;
        }

        public TransactionsBuilder WithSold(bool sold)
        {
            _transaction.Sold = sold;
            return this;
        }

        public TransactionsBuilder WithVoucherSerialNumber(string voucherSerialNumber)
        {
            _transaction.VoucherSerialNumber = voucherSerialNumber;
            return this;
        }

        public TransactionsBuilder WithVendStatusDescription(string vendStatusDescription)
        {
            _transaction.VendStatusDescription = vendStatusDescription;
            return this;
        }

        public TransactionsBuilder WithVendStatus(string vendStatus)
        {
            _transaction.VendStatus = vendStatus;
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

        public TransactionsBuilder WithStatusResponse(string statusResponse)
        {
            _transaction.StatusResponse = statusResponse;
            return this;
        }

        public TransactionsBuilder WithDateAndTimeSold(string dateAndTimeSold)
        {
            _transaction.DateAndTimeSold = dateAndTimeSold;
            return this;
        }

        public TransactionsBuilder WithDateAndTimeFinalised(string dateAndTimeFinalised)
        {
            _transaction.DateAndTimeFinalised = dateAndTimeFinalised;
            return this;
        }

        public TransactionsBuilder WithDateAndTimeLinked(string dateAndTimeLinked)
        {
            _transaction.DateAndTimeLinked = dateAndTimeLinked;
            return this;
        }

        public TransactionsBuilder WithQueryStatusCount(int queryStatusCount)
        {
            _transaction.QueryStatusCount = queryStatusCount;
            return this;
        }

        public Transaction Build()
        {
            return _transaction;
        }
    }

}
