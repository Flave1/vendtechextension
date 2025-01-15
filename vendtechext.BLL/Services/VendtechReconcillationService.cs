using FirebaseAdmin.Messaging;
using Hangfire;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts.VtchMainModels;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class VendtechReconcillationService : IVendtechReconcillationService
    {
        private readonly string _connectionString;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly EmailHelper _emailHelper;
        private readonly NotificationHelper _notificationHelper;
        public VendtechReconcillationService(IBackgroundJobClient backgroundJobClient, EmailHelper emailHelper, NotificationHelper notificationHelper)
        {
            _connectionString = "Server=92.205.181.48;Database=VENDTECH_MAIN;User Id=vendtech_main;Password=85236580@Ve;MultipleActiveResultSets=True;TrustServerCertificate=true;";
            _backgroundJobClient = backgroundJobClient;
            _emailHelper = emailHelper;
            _notificationHelper = notificationHelper;
        }

        public async Task ProcessRefundsAsync(string transactionId)
        {
            string[] transactionIds = []; //"283508"
            try
            {
                // Fetch all unsuccessful transactions
                var unsuccessfulTransactions = await GetUnsuccessfulTransactionsAsync();

                List<TransactionDetail>  filtereRecord = unsuccessfulTransactions.Where(d => !transactionIds.Contains(d.TransactionId) && d.TransactionId == transactionId).ToList();
                foreach (var transaction in filtereRecord)
                {
                    // Generate new TransactionId
                    var newTransactionId = await GenerateNewTransactionIdAsync();

                    // Refund the user
                    await RefundUserAsync(transaction.UserId, transaction.POSId, transaction.Amount, newTransactionId);

                    //Notify User
                    UserDetail user = await GetUserAsync(transaction.UserId);
                    _backgroundJobClient.Enqueue(() => CreateDepositNotification(user, transaction));
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error processing refunds: {ex.Message}");
                throw;
            }
        }

        private async Task<List<TransactionDetail>> GetUnsuccessfulTransactionsAsync()
        {
            try
            {
                var transactions = new List<TransactionDetail>();

                string query = @"
            SELECT 
                aa.Response,
                aa.UserId,
                aa.TransactionId, 
                aa.Status,
                aa.CreatedAt,
                aa.Amount, 
                aa.MeterNumber1,
                aa.POSId
                FROM [VENDTECH_MAIN].[dbo].[TransactionDetails] AS aa
                WHERE aa.Status = 1
              AND aa.PlatFormId = 1
              AND aa.CreatedAt >= CONVERT(DATETIME2, '2025-01-03 19:44:16.1647156', 121) 
              AND aa.CreatedAt <= CONVERT(DATETIME2, '2025-01-04 10:50:14.967', 121)"; //2025-01-04 01:26:01.080

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transactions.Add(new TransactionDetail
                            {
                                UserId = reader.GetInt64(reader.GetOrdinal("UserId")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                TransactionId = reader.GetString(reader.GetOrdinal("TransactionId")),
                                Status = reader.GetInt32(reader.GetOrdinal("Status")),
                                POSId = reader.GetInt64(reader.GetOrdinal("POSId")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            });
                        }
                    }
                }


                return transactions;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<UserDetail> GetUserAsync(long userId)
        {
            try
            {
                var user = new UserDetail();

                string query = @"
                    SELECT UserId, Email, Vendor
                    FROM Users
                    WHERE UserId = @UserId";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Use parameter with explicit type and size
                        command.Parameters.Add("@UserId", SqlDbType.BigInt).Value = userId;

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Use if since only one record is expected
                            {
                                user.UserId = reader.GetInt64(reader.GetOrdinal("UserId"));
                                user.Email = reader.GetString(reader.GetOrdinal("Email"));
                                user.FirstName = reader.GetString(reader.GetOrdinal("Vendor"));
                            }
                        }
                    }
                }
                return user;
            }
            catch (Exception ex)
            {
                // Log the exception (use a logging library like Serilog, NLog, etc.)
                throw new Exception("An error occurred while retrieving user details.", ex);
            }
        }


        private async Task<string> GenerateNewTransactionIdAsync()
        {
            string query = "SELECT TOP 1 TransactionId FROM Deposits ORDER BY DepositId DESC";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    if (result != null && long.TryParse(result.ToString(), out long lastTransactionId))
                    {
                        return (lastTransactionId + 1).ToString();
                    }
                    else
                    {
                        return "1";
                    }
                }
            }
        }

        private async Task RefundUserAsync(long userId, long posId, decimal amount, string newTransactionId)
        {
            string getPosBalanceQuery = @"
                SELECT Balance
                FROM POS
                WHERE VendorId = @UserId AND POSId = @POSId";

            string updatePosQuery = @"
                UPDATE POS
                SET Balance = Balance + @Amount
                WHERE VendorId = @UserId AND POSId = @POSId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        decimal balanceBefore;
                        decimal newBalance;

                        // Fetch the current balance of the POS
                        using (SqlCommand getBalanceCommand = new SqlCommand(getPosBalanceQuery, connection, transaction))
                        {
                            getBalanceCommand.Parameters.AddWithValue("@UserId", userId);
                            getBalanceCommand.Parameters.AddWithValue("@POSId", posId);

                            var result = await getBalanceCommand.ExecuteScalarAsync();
                            if (result == null)
                            {
                                throw new Exception($"POS record not found for UserId: {userId}, POSId: {posId}");
                            }

                            balanceBefore = Convert.ToDecimal(result);
                            newBalance = balanceBefore + amount;
                        }

                        // Update POS balance
                        using (SqlCommand updateCommand = new SqlCommand(updatePosQuery, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@Amount", amount);
                            updateCommand.Parameters.AddWithValue("@UserId", userId);
                            updateCommand.Parameters.AddWithValue("@POSId", posId);

                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        // Insert refund record
                        string insertRefundQuery = GenerateInsertDepositQuery(
                            userId: userId,
                            posId: posId,
                            createdAt: DateTime.Now,
                            transactionId: newTransactionId,
                            paymentType: 1,
                            balanceBefore: balanceBefore,
                            amount: amount,
                            percentageAmount: amount,
                            newBalance: newBalance,
                            agencyCommission: 0,
                            checkNumberOrSlipId: "SALES REVERSAL",
                            comments: "SALES REVERSAL",
                            status: 1,
                            chequeBankName: "VENDTECHSL",
                            nameOnCheque: null,
                            updatedAt: null,
                            bankAccountId: 1,
                            isAudit: true,
                            valueDate: DateTime.Now.ToString(),
                            nextReminderDate: null,
                            isDeleted: false,
                            valueDateStamp: null,
                            initiatingTransactionId: null
                        );

                        using (SqlCommand insertCommand = new SqlCommand(insertRefundQuery, connection, transaction))
                        {
                            await insertCommand.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public string GenerateInsertDepositQuery(
                        long userId,
                        long posId,
                        DateTime createdAt,
                        string transactionId,
                        int paymentType,
                        decimal balanceBefore,
                        decimal amount,
                        decimal? percentageAmount,
                        decimal newBalance,
                        decimal? agencyCommission,
                        string checkNumberOrSlipId,
                        string comments,
                        int status,
                        string chequeBankName,
                        string nameOnCheque,
                        DateTime? updatedAt,
                        int bankAccountId,
                        bool isAudit,
                        string valueDate,
                        DateTime? nextReminderDate,
                        bool isDeleted,
                        DateTime? valueDateStamp,
                        string initiatingTransactionId)
        {
            // Start building the query
            var query = new StringBuilder("INSERT INTO Deposits (");

            query.Append("[UserId], [POSId], [CreatedAt], [TransactionId], [PaymentType], [BalanceBefore], ");
            query.Append("[Amount], [PercentageAmount], [NewBalance], [AgencyCommission], [CheckNumberOrSlipId], ");
            query.Append("[Comments], [Status], [ChequeBankName], [NameOnCheque], [UpdatedAt], [BankAccountId], ");
            query.Append("[isAudit], [ValueDate], [NextReminderDate], [IsDeleted], [ValueDateStamp], [InitiatingTransactionId]) ");
            query.Append("VALUES (");

            // Add parameterized values
            query.Append($"{userId}, ");
            query.Append($"{posId}, ");
            query.Append($"'{createdAt:yyyy-MM-dd HH:mm:ss}', ");
            query.Append(transactionId == null ? "NULL, " : $"'{transactionId}', ");
            query.Append($"{paymentType}, ");
            query.Append($"{balanceBefore}, ");
            query.Append($"{amount}, ");
            query.Append(percentageAmount == null ? "NULL, " : $"{percentageAmount}, ");
            query.Append($"{newBalance}, ");
            query.Append(agencyCommission == null ? "NULL, " : $"{agencyCommission}, ");
            query.Append($"'{checkNumberOrSlipId}', ");
            query.Append(comments == null ? "NULL, " : $"'{comments.Replace("'", "''")}', ");
            query.Append($"{status}, ");
            query.Append(chequeBankName == null ? "NULL, " : $"'{chequeBankName.Replace("'", "''")}', ");
            query.Append(nameOnCheque == null ? "NULL, " : $"'{nameOnCheque.Replace("'", "''")}', ");
            query.Append(updatedAt == null ? "NULL, " : $"'{updatedAt:yyyy-MM-dd HH:mm:ss}', ");
            query.Append($"{bankAccountId}, ");
            query.Append($"{(isAudit ? 1 : 0)}, ");
            query.Append(valueDate == null ? "NULL, " : $"'{valueDate}', ");
            query.Append(nextReminderDate == null ? "NULL, " : $"'{nextReminderDate:yyyy-MM-dd HH:mm:ss}', ");
            query.Append($"{(isDeleted ? 1 : 0)}, ");
            query.Append(valueDateStamp == null ? "NULL, " : $"'{valueDateStamp:yyyy-MM-dd HH:mm:ss}', ");
            query.Append(initiatingTransactionId == null ? "NULL" : $"'{initiatingTransactionId}'");

            query.Append(");");

            return query.ToString();
        }


        public void CreateDepositNotification(UserDetail user, TransactionDetail transaction)
        {
            new Emailer(_emailHelper, _notificationHelper).SendReconcilationEmail(user, transaction);
        }
    }

  
}
