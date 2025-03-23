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
        public readonly DataContext _context;
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
                    .SetParentDepositId(dto.CommissionDepositId)
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
            deposit.Status = (int)DepositStatus.Approved;
            deposit.CommissionDeposit.Status = (int)DepositStatus.Approved;
            await _context.SaveChangesAsync();
            return deposit;
        }

        public async Task<Deposit> GetDepositTransaction(Guid Id)
        {
            var trans = await _context.Deposits.Include(d => d.CommissionDeposit).FirstOrDefaultAsync(d => d.Id == Id && d.Deleted == false) ?? null;
            if (trans == null)
                throw new BadRequestException("Unable to find deposit");
            return trans;
        }
        public async Task CancelDepositTransaction(Deposit deposit)
        {
            if (deposit == null)
                throw new BadRequestException("Unable to find deposit");
            deposit.Status = (int)DepositStatus.Cancelled;
            deposit.CommissionDeposit.Status = (int)DepositStatus.Cancelled;
            await _context.SaveChangesAsync();
        }
        public async Task<List<LastDeposit>> GetLastDepositTransaction(Guid integratorId)
        {
            var trans = await _context.Deposits.Where(d => d.IntegratorId == integratorId && d.Deleted == false)
                .Include(t => t.PaymentMethod)
                .OrderByDescending(d => d.CreatedAt).Take(10)
                .Select(d => new LastDeposit
                {
                    Amount = d.Amount,
                    Date = Utils.formatDate(d.CreatedAt),
                    Reference = d.Reference,
                    TransactionId = d.TransactionId,
                    Status = d.Status,
                    PaymentTypeName = d.PaymentMethod.Name
                })
                .ToListAsync() ?? new List<LastDeposit>();
            return trans;
        }

        public async Task<List<PaymentTypeDto>> GetPaymentTypes()
        {
            return await _context.PaymentMethod.Where(d => d.Deleted == false && d.Type == (int)PaymentMethodType.External).Select(f => new PaymentTypeDto { 
            Description = f.Description,
            Id  = f.Id,
            Name = f.Name,
            }).ToListAsync();
        }

        public async Task<PaymentTypeDto> GetCommissionType()
        {
            return await _context.PaymentMethod.Where(d => d.Deleted == false && d.Type == (int)PaymentMethodType.Internal).Select(f => new PaymentTypeDto
            {
                Description = f.Description,
                Id = f.Id,
                Name = f.Name,
            }).FirstOrDefaultAsync();
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

        private async Task<bool> TransactionAlreadyExist(Guid integratorId, string transactionUniqueId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT COUNT(1) 
                FROM Transactions 
                WHERE IntegratorId = @integratorId AND TransactionUniqueId = @transactionUniqueId;";

                    var integratorIdParam = command.CreateParameter();
                    integratorIdParam.ParameterName = "@integratorId";
                    integratorIdParam.Value = integratorId;
                    command.Parameters.Add(integratorIdParam);

                    var transactionUniqueIdParam = command.CreateParameter();
                    transactionUniqueIdParam.ParameterName = "@transactionUniqueId";
                    transactionUniqueIdParam.Value = transactionUniqueId;
                    command.Parameters.Add(transactionUniqueIdParam);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }


        public async Task SalesInternalValidation(Wallet wallet, ElectricitySaleRequest request, Guid integratorid)
        {
            SettingsPayload settings = AppConfiguration.GetSettings();
            int minimumVend = settings.Threshholds.MinimumVend;

            if (request.Amount <= 0)
                throw new BadRequestException($"Amount is required.");

            if (string.IsNullOrEmpty(request.MeterNumber))
                throw new BadRequestException($"Meter Number is required.");

            if (request.MeterNumber.Length != 11)
                throw new BadRequestException($"Meter Number must be 11 Numbers.");

            if (string.IsNullOrEmpty(request.TransactionId))
                throw new BadRequestException($"Transaction Id is required.");

            if (request.Amount < minimumVend)
                throw new BadRequestException($"Provided amount can not be below {minimumVend}.");

            if (await TransactionAlreadyExist(integratorid, request.TransactionId))
                throw new BadRequestException("Transaction ID already exist for this terminal.");

            if(request.Amount > wallet.Balance)
                throw new BadRequestException("Insufficient Balance");

            if (settings.DisableElectricitySales)
                throw new SystemDisabledException("Electricity vending is currently disabled.");

            if(!string.IsNullOrEmpty(request.Simulate) && DomainEnvironment.IsProduction)
                throw new BadRequestException("Request cannot be simulated in a production environment.");
        }


        public async Task<Transaction> DeductFromWallet(Guid transactionId, Guid walletId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                await using (var command = connection.CreateCommand())
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

                    await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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

        public async Task<Transaction> GetSaleTransactionByRandom(ElectricitySaleRequest request)
        {
            string successTranxId = "311567";
            string pendingTranxId = "312742";
            string failedTranxId = "312740";

            Transaction trans = null;
            if (string.IsNullOrEmpty(request.Simulate) || request.Simulate?.ToLower() == "success")
                trans = await _context.Transactions
                .Where(d => d.VendtechTransactionID == successTranxId)
                .FirstOrDefaultAsync();

            else if (request.Simulate.ToLower() == "pending")
                trans = await _context.Transactions
                .Where(d => d.VendtechTransactionID == pendingTranxId)
                .FirstOrDefaultAsync();

            else if (request.Simulate.ToLower() == "failed")
                trans = await _context.Transactions
                .Where(d => d.VendtechTransactionID == failedTranxId)
                .FirstOrDefaultAsync();
            else
                throw new BadRequestException("Invalid simulation value.");

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

    #region TRANSACTION UPDATE
    public class TransactionUpdate
    {
        private readonly DataContext _context;

        public TransactionUpdate(DataContext context)
        {
            _context = context;
        }

        public async Task UpdateSaleSuccessTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            trans = new TransactionsBuilder(trans)
                .WithSellerReturnedBalance(executionResult.successResponse?.Voucher?.SellerReturnedBalance ?? 0)
                .WithVendStatusDescription(executionResult.successResponse.Voucher?.VendStatusDescription ?? "")
                .WithSellerTransactionId(executionResult.successResponse.Voucher?.SellerTransactionID ?? "")
                .WithSerialNumber(executionResult.successResponse?.Voucher?.VoucherSerialNumber)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithReceivedFrom(executionResult.receivedFrom)
                .WithResponse(executionResult.response)
                .WithRequest(executionResult.request)
                .WithFinalized(true)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSuceessSaleTransactionLogOnStatusQuery(ExecutionResult executionResult, Transaction trans)
        {
            trans = new TransactionsBuilder(trans)
                .WithSellerTransactionId(executionResult.successResponse.Voucher?.SellerTransactionID ?? "")
                .WithQueryStatusMessage(executionResult.successResponse.Voucher?.VendStatusDescription ?? "")
                .WithSerialNumber(executionResult.successResponse?.Voucher?.VoucherSerialNumber)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithReceivedFrom(executionResult.receivedFrom)
                .WithStatusResponse(executionResult.response)
                .WithStatusRequest(executionResult.request)
                .WithFinalized(true)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateFailedSaleTransactionLogOnStatusQuery(ExecutionResult executionResult, Transaction trans)
        {
            trans = new TransactionsBuilder(trans)
                   .WithQueryStatusMessage(executionResult.successResponse.Voucher?.VendStatusDescription ?? "")
                   .WithTransactionStatus(TransactionStatus.Failed)
                   .WithReceivedFrom(executionResult.receivedFrom)
                   .WithStatusResponse(executionResult.response)
                   .WithStatusRequest(executionResult.request)
                   .Build();

            await _context.SaveChangesAsync();
        }


        public async Task UpdateSaleFailedTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            trans = new TransactionsBuilder(trans)
               .WithVendStatusDescription(executionResult.failedResponse.ErrorDetail)
               .WithTransactionStatus(TransactionStatus.Failed)
               .WithReceivedFrom(executionResult.receivedFrom)
               .WithResponse(executionResult.response)
               .WithBalanceAfter(trans.BalanceBefore)
               .WithRequest(executionResult.request)
               .Build();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSalePendingTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            trans = new TransactionsBuilder(trans)
               .WithVendStatusDescription(executionResult.failedResponse.ErrorDetail)
               .WithTransactionStatus(TransactionStatus.Pending)
               .WithReceivedFrom(executionResult.receivedFrom)  
               .WithResponse(executionResult.response)
               .WithBalanceAfter(trans.BalanceBefore)
               .WithRequest(executionResult.request)
               .Build();

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSaleSuccessTransactionLogSANDBOX(Transaction existingTrans, Transaction newTrans)
        {
            newTrans = new TransactionsBuilder(newTrans)
                .WithSellerReturnedBalance(existingTrans.SellerReturnedBalance)
                .WithVendStatusDescription(existingTrans.VendStatusDescription ?? "")
                .WithSellerTransactionId(existingTrans.SellerTransactionID ?? "")
                .WithSerialNumber(existingTrans.VoucherSerialNumber)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithReceivedFrom(existingTrans.ReceivedFrom)
                .WithResponse(existingTrans.Response)
                .WithRequest(existingTrans.Request)
                .WithFinalized(true)
                .Build();

            await _context.SaveChangesAsync();
        }

    }
    #endregion
}
