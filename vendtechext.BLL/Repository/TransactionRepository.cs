using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using vendtechext.BLL.Common;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Services;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Migrations;
using vendtechext.DAL.Models;
using vendtechext.Helper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace vendtechext.BLL.Repository
{
    public class TransactionRepository
    {
        private readonly DataContext _context;
        private readonly VendtechTransactionsService _vendtech;
        private readonly List<PaymentType> _types = new List<PaymentType>
            {
                new PaymentType{ Id = 1, Name = "BANK DEPOSIT", Description = "A payment method where funds are deposited directly into a bank account through a branch or electronic means."},
                new PaymentType{ Id = 2, Name = "TRANSFER", Description = "An electronic method of transferring funds between accounts, typically using bank services or third-party platforms."},
                new PaymentType{ Id = 3, Name = "CASH", Description = "A physical payment made using paper currency or coins, often handled in person for immediate transactions."},
            };

        public TransactionRepository(DataContext context, VendtechTransactionsService vendtech)
        {
            _context = context;
            _vendtech = vendtech;
        }

        #region COMMON
        public TodaysTransaction GetTodaysTransaction(Guid integratorId)
        {
            var todaysDate = DateTime.UtcNow.Date;

            return new TodaysTransaction
            {
                Deposits = _context.Transactions.FirstOrDefault(d => d.Deleted == false
                && d.IntegratorId == integratorId && d.TransactionStatus == (int)TransactionStatus.Success
                && d.CreatedAt.Date == todaysDate).Amount,
                Sales = _context.Wallets.FirstOrDefault(d => d.Deleted == false
                && d.IntegratorId == integratorId  && d.CreatedAt.Date == todaysDate).Balance
            };
        }
        #endregion

        #region DEPOSITS TRANSACTION REGION


        public async Task<Deposit> CreateDepositTransaction(CreateDepositDto dto, DepositStatus status)
        {
            var deposit = new DepositBuilder()
                    .SetTransactionId(UniqueIDGenerator.NewDepositTransactionId())
                    .SetBalanceBefore(dto.BalanceBefore)
                    .SetPaymentTypeId(dto.PaymentTypeId)
                    .SetIntegratorId(dto.IntegratorId)
                    .SetBalanceAfter(dto.BalanceAfter)
                    .SetReference(dto.Reference)
                    .SetAmount(dto.Amount)
                    .SetStatus(status)
                    .Build();

            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();
            return deposit;
        }

        public async Task<Deposit> ApproveDepositTransaction(Deposit deposit)
        {
            deposit = new DepositBuilder(deposit)
                    .SetStatus(DepositStatus.Approved)
                    .Build();

            await _context.SaveChangesAsync();
            return deposit;
        }

        public async Task<Deposit> GetDepositTransaction(Guid Id)
        {
            var trans = await _context.Deposits.FirstOrDefaultAsync(d => d.Id == Id && d.Deleted == false) ?? null;
            if (trans == null)
                throw new BadRequestException("Unable to find deposit");
            return trans;
        }
        public async Task DeleteDepositTransaction(Deposit deposit)
        {
            if (deposit == null)
                throw new BadRequestException("Unable to find deposit");
            deposit.Deleted = true;
            await _context.SaveChangesAsync();
        }
        public async Task<List<LastDeposit>> GetLastDepositTransaction(Guid integratorId)
        {
            var trans = await _context.Deposits.Where(d => d.IntegratorId == integratorId && d.Deleted == false).OrderByDescending(d => d.CreatedAt).Take(10)
                .Select(d => new LastDeposit
                {
                    Amount = d.Amount,
                    Date = Utils.formatDate(d.CreatedAt),
                    Reference = d.Reference,
                    TransactionId = d.TransactionId,
                    Status = d.Status
                })
                .ToListAsync() ?? new List<LastDeposit>();
            return trans;
        }

        public async Task<List<PaymentType>> GetPaymentTypes()
        {
            return await Task.Run(() => _types);
        }

        public IQueryable<Deposit> GetDepositsQuery(DepositStatus status)
        {
            IQueryable<Deposit> query = _context.Deposits.Where(d => d.Deleted == false && d.Status == (int)status)
                .Include(t => t.Integrator).ThenInclude(d => d.Wallet);
            return query;
        }

        #endregion

        #region SALES TRANSACTIONS REGION

        public async Task UpdateSaleSuccessTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithVendStatusDescription(executionResult.SuccessResponse.Voucher.VendStatusDescription)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithReceivedFrom(executionResult.ReceivedFrom)
                .WithResponse(executionResult.Response)
                .WithRequest(executionResult.Request)
                .WithFinalized(true)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSaleFailedTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithVendStatusDescription(executionResult.FailedResponse.ErrorDetail)
                .WithTransactionStatus(TransactionStatus.Failed)
                .WithReceivedFrom(executionResult.ReceivedFrom)
                .WithResponse(executionResult.Response)
                .WithBalanceAfter(trans.BalanceBefore)
                .WithRequest(executionResult.Request)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task SalesInternalValidation(Wallet wallet, ElectricitySaleRequest request, Guid integratorid)
        {
            SettingsPayload settings = AppConfiguration.GetSettings();
            int minimumVend = settings.Threshholds.MinimumVend;
            if(request.Amount < minimumVend)
                throw new BadRequestException($"Provided amount can not be below {minimumVend}");

            if (await _context.Transactions.AnyAsync(d => d.IntegratorId == integratorid && d.TransactionUniqueId == request.TransactionId))
                throw new BadRequestException("Transaction ID already exist for this terminal.");

            if(request.Amount > wallet.Balance)
                throw new BadRequestException("Insufficient Balance");
        }

        public async Task DeductFromWallet(Wallet wallet, Transaction transaction)
        {
            transaction.BalanceBefore = wallet.Balance;
            wallet.Balance = (wallet.Balance - transaction.Amount);
            transaction.BalanceAfter = wallet.Balance;
            await _context.SaveChangesAsync();
        }
        public async Task RefundToWallet(Wallet wallet, Transaction transaction)
        {
            wallet.Balance = (wallet.Balance + transaction.Amount);
            await _context.SaveChangesAsync();
        }
        public async Task<Transaction> GetSaleTransaction(string transactionId, Guid integratorid)
        {
            var trans = await _context.Transactions.FirstOrDefaultAsync(d => d.TransactionUniqueId == transactionId && d.IntegratorId == integratorid) ?? null;
            return trans;
        }

        public async Task<Transaction> CreateSaleTransactionLog(ElectricitySaleRequest request, Guid integratorId)
        {
            //dynamic transaction = await _vendtech.CreateRecordBeforeVend(request.MeterNumber, request.Amount);
            string transactionId = UniqueIDGenerator.NewSaleTransactionId();
            string newTrxid = transactionId;// request.TransactionId; //264713

            var trans = new TransactionsBuilder()
                .WithTransactionId(newTrxid)
                .WithTransactionStatus(TransactionStatus.Pending)
                .WithTransactionUniqueId(request.TransactionId)
                .WithMeterNumber(request.MeterNumber)
                .WithIntegratorId(integratorId)
                .WithCreatedAt(DateTime.Now)
                .WithAmount(request.Amount)
                .Build();

            _context.Transactions.Add(trans);
            await _context.SaveChangesAsync();
            return trans;
        }

        public IQueryable<Transaction> GetSalesTransactionQuery(int status, int claimedStatus)
        {
            if(status != (int)TransactionStatus.All)
            {
                var query = _context.Transactions.Where(d => d.Deleted == false && d.TransactionStatus == status)
                    .Include(d => d.Integrator).ThenInclude(d => d.Wallet);
                return query;
            }
            else
            {
                var query = _context.Transactions.Where(d => d.Deleted == false)
                            .Include(d => d.Integrator).ThenInclude(d => d.Wallet);
                return query;
            }
        }

        #endregion

    }
}
