using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using vendtechext.BLL.Common;
using vendtechext.BLL.Exceptions;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Repository
{
    public class TransactionRepository
    {
        private readonly DataContext _context;
        private readonly LogService _logService;
        private readonly TransactionIdGenerator _idgenerator;
        private readonly IConfiguration _configuration;

        public TransactionRepository(DataContext context, LogService logService, TransactionIdGenerator idgenerator, IConfiguration configuration)
        {
            _context = context;
            _logService = logService;
            _idgenerator = idgenerator;
            _configuration = configuration;
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

        public async Task<List<PaymentTypeDto>> GetPaymentTypes()
        {
            return await _context.PaymentMethod.Where(d => d.Deleted == false).Select(f => new PaymentTypeDto { 
            Description = f.Description,
            Id  = f.Id,
            Name = f.Name,
            }).ToListAsync();
        }

        public IQueryable<Deposit> GetDepositsQuery(DepositStatus status)
        {
            IQueryable<Deposit> query = _context.Deposits.Where(d => d.Deleted == false && d.Status == (int)status)
                .Include(d => d.PaymentMethod)
                .Include(t => t.Integrator).ThenInclude(d => d.Wallet);
            return query;
        }

        #endregion

        #region SALES TRANSACTIONS REGION

        public async Task UpdateSaleSuccessTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithSellerReturnedBalance(executionResult.successResponse.Voucher.SellerReturnedBalance == null? 0 : executionResult.successResponse.Voucher.SellerReturnedBalance.Value)
                .WithVendStatusDescription(executionResult.successResponse.Voucher?.VendStatusDescription ?? "")
                .WithSellerTransactionId(executionResult.successResponse.Voucher?.RTSUniqueID ?? "")
                .WithTransactionStatus(TransactionStatus.Success)
                .WithReceivedFrom(executionResult.receivedFrom)
                .WithResponse(executionResult.response)
                .WithRequest(executionResult.request)
                .WithFinalized(true)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSaleTransactionLogOnStatusQuery(ExecutionResult executionResult, Transaction trans, TransactionStatus status)
        {
            new TransactionsBuilder(trans)
                .WithQueryStatusMessage(executionResult.successResponse.Voucher?.VendStatusDescription ?? "")
                .WithTransactionStatus(status)
                .WithReceivedFrom(executionResult.receivedFrom)
                .WithResponse(executionResult.response)
                .WithRequest(executionResult.request)
                .Build();

            await _context.SaveChangesAsync();
        }

     
        public async Task UpdateSaleFailedTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithVendStatusDescription(executionResult?.failedResponse?.ErrorDetail)
                .WithTransactionStatus(TransactionStatus.Failed)
                .WithReceivedFrom(executionResult?.receivedFrom)
                .WithResponse(executionResult?.response)
                .WithBalanceAfter(trans.BalanceBefore)
                .WithRequest(executionResult?.request)
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

            if (settings.DisableElectricitySales)
                throw new SystemDisabledException("Electricity vending is currently disabled");
        }

        //public async Task DeductFromWallet(Wallet wallet, Transaction transaction)
        //{
        //    if(transaction.PaymentStatus == (int)PaymentStatus.Pending)
        //    {
        //        transaction.BalanceBefore = wallet.Balance;
        //        wallet.Balance = (wallet.Balance - transaction.Amount);
        //        transaction.BalanceAfter = wallet.Balance;
        //        transaction.PaymentStatus = (int)PaymentStatus.Deducted;
        //        await _context.SaveChangesAsync();
        //    }
        //}

        public async Task<Transaction> DeductFromWallet(Guid transactionId, Guid walletId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    // Step 1: Retrieve transaction and wallet details
                    command.CommandText = @"
                    SELECT Amount, PaymentStatus FROM Transactions WHERE Id = @transactionId;
                    SELECT Balance FROM Wallets WHERE Id = @walletId;";

                    var transactionIdParam = command.CreateParameter();
                    transactionIdParam.ParameterName = "@transactionId";
                    transactionIdParam.Value = transactionId;
                    command.Parameters.Add(transactionIdParam);

                    var walletIdParam = command.CreateParameter();
                    walletIdParam.ParameterName = "@walletId";
                    walletIdParam.Value = walletId;
                    command.Parameters.Add(walletIdParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read()) throw new ServerTechnicalException("Unable to find transaction");
                        decimal amount = reader.GetDecimal(0);
                        int paymentStatus = reader.GetInt32(1);

                        if (!reader.NextResult() || !reader.Read()) throw new ServerTechnicalException("Unable to find wallet"); // Wallet not found
                        decimal walletBalance = reader.GetDecimal(0);

                        if (paymentStatus == (int)PaymentStatus.Pending)
                        {
                            // Step 2: Deduct balance and update transaction
                            reader.Close(); // Close the reader before executing another command

                            command.CommandText = @"
                            UPDATE Wallets SET Balance = @newBalance WHERE Id = @walletId;
                            UPDATE Transactions 
                            SET BalanceBefore = @balanceBefore, 
                                BalanceAfter = @newBalance, 
                                PaymentStatus = @newStatus 
                            WHERE Id = @transactionId;";

                            var newBalance = walletBalance - amount;

                            command.Parameters.Clear();
                            command.Parameters.Add(new SqlParameter("@newBalance", newBalance));
                            command.Parameters.Add(new SqlParameter("@balanceBefore", walletBalance));
                            command.Parameters.Add(new SqlParameter("@newStatus", (int)PaymentStatus.Deducted));
                            command.Parameters.Add(new SqlParameter("@transactionId", transactionId));
                            command.Parameters.Add(new SqlParameter("@walletId", walletId));

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            return await _context.Transactions.FindAsync(transactionId);
        }


        //public async Task DeductFromWalletIfRefunded(Wallet wallet, Transaction transaction)
        //{
        //    if (transaction.PaymentStatus == (int)PaymentStatus.Refunded)
        //    {
        //        transaction.BalanceBefore = wallet.Balance;
        //        wallet.Balance = (wallet.Balance - transaction.Amount);
        //        transaction.BalanceAfter = wallet.Balance;
        //        transaction.PaymentStatus = (int)PaymentStatus.Deducted;
        //        await _context.SaveChangesAsync();
        //        _logService.Log(LogType.Refund, $"fund_claimed {transaction.Amount} to {wallet.WALLET_ID} " + $"for {transaction.VendtechTransactionID} ID", transaction?.Response ?? "");
        //    }
        //}

        public async Task<Transaction> DeductFromWalletIfRefunded(Guid transactionId, Guid walletId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    // Step 1: Retrieve transaction and wallet details
                    command.CommandText = @"
                        SELECT Amount, PaymentStatus, VendtechTransactionID, Response 
                        FROM Transactions WHERE Id = @transactionId;

                        SELECT Balance FROM Wallets WHERE Id = @walletId;";

                    var transactionIdParam = command.CreateParameter();
                    transactionIdParam.ParameterName = "@transactionId";
                    transactionIdParam.Value = transactionId;
                    command.Parameters.Add(transactionIdParam);

                    var walletIdParam = command.CreateParameter();
                    walletIdParam.ParameterName = "@walletId";
                    walletIdParam.Value = walletId;
                    command.Parameters.Add(walletIdParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read()) throw new ServerTechnicalException("Unable to find transaction");
                        decimal amount = reader.GetDecimal(0);
                        int paymentStatus = reader.GetInt32(1);
                        string vendtechTransactionID = reader.GetString(2);
                        string response = reader.IsDBNull(3) ? "" : reader.GetString(3);

                        if (!reader.NextResult() || !reader.Read()) throw new ServerTechnicalException("Unable to find wallet");
                        decimal walletBalance = reader.GetDecimal(0);

                        if (paymentStatus == (int)PaymentStatus.Refunded)
                        {
                            // Step 2: Deduct balance and update transaction
                            reader.Close(); // Close the reader before executing another command

                            command.CommandText = @"
                                UPDATE Wallets SET Balance = @newBalance WHERE Id = @walletId;
                                UPDATE Transactions 
                                SET BalanceBefore = @balanceBefore, 
                                    BalanceAfter = @newBalance, 
                                    PaymentStatus = @newStatus 
                                WHERE Id = @transactionId;";

                            var newBalance = walletBalance - amount;

                            command.Parameters.Clear();
                            command.Parameters.Add(new SqlParameter("@newBalance", newBalance));
                            command.Parameters.Add(new SqlParameter("@balanceBefore", walletBalance));
                            command.Parameters.Add(new SqlParameter("@newStatus", (int)PaymentStatus.Deducted));
                            command.Parameters.Add(new SqlParameter("@transactionId", transactionId));
                            command.Parameters.Add(new SqlParameter("@walletId", walletId));

                            await command.ExecuteNonQueryAsync();

                            // Step 3: Log the transaction
                            _logService.Log(LogType.Refund,
                                $"fund_claimed {amount} to {walletId} for {vendtechTransactionID} ID",
                                response);
                        }
                    }
                }
            }
            return await _context.Transactions.FindAsync(transactionId);
        }


        //public async Task RefundToWallet(Wallet wallet, Transaction transaction)
        //{
        //    if(transaction.PaymentStatus == (int)PaymentStatus.Deducted)
        //    {
        //        wallet.Balance = (wallet.Balance + transaction.Amount);
        //        transaction.PaymentStatus = (int)PaymentStatus.Refunded;
        //        transaction.BalanceAfter = transaction.BalanceBefore;
        //        await _context.SaveChangesAsync();
        //        _logService.Log(LogType.Refund, $"refunded {transaction.Amount} to {wallet.WALLET_ID} " + $"for {transaction.VendtechTransactionID} ID", transaction?.Response ?? "");
        //    }
        //    else
        //    {
        //        _logService.Log(LogType.Refund, $"attempted refund {transaction.Amount} to {wallet.WALLET_ID} " +  $"for {transaction.VendtechTransactionID} ID", transaction?.Response ?? "");
        //    }
        //}

        public async Task<Transaction> RefundToWallet(Guid transactionId, Guid walletId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    // Step 1: Retrieve transaction and wallet details
                    command.CommandText = @"
                            SELECT Amount, PaymentStatus, BalanceBefore, VendtechTransactionID, Response 
                            FROM Transactions WHERE Id = @transactionId;

                            SELECT Balance FROM Wallets WHERE Id = @walletId;";

                    var transactionIdParam = command.CreateParameter();
                    transactionIdParam.ParameterName = "@transactionId";
                    transactionIdParam.Value = transactionId;
                    command.Parameters.Add(transactionIdParam);

                    var walletIdParam = command.CreateParameter();
                    walletIdParam.ParameterName = "@walletId";
                    walletIdParam.Value = walletId;
                    command.Parameters.Add(walletIdParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read()) throw new ServerTechnicalException("Unable to find transaction");
                        decimal amount = reader.GetDecimal(0);
                        int paymentStatus = reader.GetInt32(1);
                        decimal balanceBefore = reader.GetDecimal(2);
                        string vendtechTransactionID = reader.GetString(3);
                        string response = reader.IsDBNull(4) ? "" : reader.GetString(4);

                        if (!reader.NextResult() || !reader.Read()) throw new ServerTechnicalException("Unable to find wallet");
                        decimal walletBalance = reader.GetDecimal(0);

                        if (paymentStatus == (int)PaymentStatus.Deducted)
                        {
                            // Step 2: Refund balance and update transaction
                            reader.Close(); // Close the reader before executing another command

                            command.CommandText = @"
                                UPDATE Wallets SET Balance = @newBalance WHERE Id = @walletId;
                                UPDATE Transactions 
                                SET PaymentStatus = @newStatus, 
                                    BalanceAfter = @balanceAfter 
                                WHERE Id = @transactionId;";

                            var newBalance = walletBalance + amount;

                            command.Parameters.Clear();
                            command.Parameters.Add(new SqlParameter("@newBalance", newBalance));
                            command.Parameters.Add(new SqlParameter("@balanceAfter", balanceBefore));
                            command.Parameters.Add(new SqlParameter("@newStatus", (int)PaymentStatus.Refunded));
                            command.Parameters.Add(new SqlParameter("@transactionId", transactionId));
                            command.Parameters.Add(new SqlParameter("@walletId", walletId));

                            await command.ExecuteNonQueryAsync();

                            // Step 3: Log the successful refund
                            _logService.Log(LogType.Refund,
                                $"refunded {amount} to {walletId} for {vendtechTransactionID} ID",
                                response);
                        }
                        else
                        {
                            // Step 4: Log the failed refund attempt
                            _logService.Log(LogType.Refund,
                                $"attempted refund {amount} to {walletId} for {vendtechTransactionID} ID",
                                response);
                        }
                    }
                }
            }
            return await _context.Transactions.FindAsync(transactionId);
        }

        public async Task<Transaction> GetSaleTransaction(string transactionId, Guid integratorid)
        {
            var trans = await _context.Transactions.FirstOrDefaultAsync(d => d.TransactionUniqueId == transactionId && d.IntegratorId == integratorid) ?? null;
            return trans;
        }

        public async Task<Transaction> GetSaleTransactionByRandom(string meterNumber)
        {
            string[] transactionIds = ["268085", "268027", "268029", "268025", "268023", "265514", "265511", "264840"];
            string randomTransactionId = transactionIds[new Random().Next(transactionIds.Length)];

            var trans = await _context.Transactions
                .Where(d => d.VendtechTransactionID == randomTransactionId)
                .FirstOrDefaultAsync();

            return trans;
        }

        public async Task<Transaction> CreateSaleTransactionLog(ElectricitySaleRequest request, Guid integratorId)
        {
            string transactionId = await _idgenerator.GenerateNewTransactionId();
            string newTrxid = transactionId;

            var trans = new TransactionsBuilder()
                .WithTransactionStatus(TransactionStatus.Pending)
                .WithTransactionUniqueId(request.TransactionId)
                .WithPaymentStatus(PaymentStatus.Pending)
                .WithMeterNumber(request.MeterNumber)
                .WithIntegratorId(integratorId)
                .WithSellerReturnedBalance(0)
                .WithCreatedAt(DateTime.Now)
                .WithTransactionId(newTrxid)
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
