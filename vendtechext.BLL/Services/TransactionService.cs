using Microsoft.Data.SqlClient;
using vendtechext.BLL.Common;

namespace vendtechext.BLL.Services
{
    public class VendtechTransactionsService
    {
        private readonly string _connectionString;

        public VendtechTransactionsService()
        {
            _connectionString = "Server=92.205.181.48;Database=VENDTECH_MAIN;User Id=vendtech_main;Password=85236580@Ve;MultipleActiveResultSets=True;TrustServerCertificate=true;";
        }

        public async Task<dynamic> CreateRecordBeforeVend(string meterNumber, decimal amount)
        {
            var transaction = new 
            {
                PlatFormId = 1,
                UserId = 40251,
                POSId = 20034,
                MeterNumber1 = meterNumber,
                Amount = amount,
                IsDeleted = false,
                Status = 2,
                CreatedAt = DateTime.UtcNow,
                RTSUniqueID = "00",
                TenderedAmount = amount,
                TransactionAmount = amount,
                Finalised = false,
                StatusRequestCount = 0,
                Sold = false,
                DebitRecovery = "0",
                CostOfUnits = "0",
                TransactionId = await GenerateNewTransactionId(),
                RequestDate = DateTime.UtcNow,
                CurrentDealerBalance = 0,
                TaxCharge = 0,
                Units = 0,
            };

            // Build the SQL query
            var query = @"INSERT INTO TransactionDetails 
                     (PlatFormId, UserId, ReceivedFrom, MeterNumber1, Amount, 
                      IsDeleted, Status, CreatedAt, RTSUniqueID, TenderedAmount, 
                      TransactionAmount, Finalised, StatusRequestCount, Sold, DebitRecovery, 
                      CostOfUnits, VendtechTransactionId, RequestDate, CurrentDealerBalance, TaxCharge, Units)
                      VALUES 
                      (@PlatFormId, @UserId, @ReceivedFrom, @MeterNumber1, @Amount,
                       @IsDeleted, @Status, @CreatedAt, @RTSUniqueID, @TenderedAmount, 
                       @TransactionAmount, @Finalised, @StatusRequestCount, @Sold, @DebitRecovery, 
                       @CostOfUnits, @VendtechTransactionId, @RequestDate, @CurrentDealerBalance, @TaxCharge, @Units)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@PlatFormId", transaction.PlatFormId);
                    command.Parameters.AddWithValue("@UserId", transaction.UserId);
                    command.Parameters.AddWithValue("@ReceivedFrom", transaction.POSId);
                    command.Parameters.AddWithValue("@MeterNumber1", transaction.MeterNumber1);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@IsDeleted", transaction.IsDeleted);
                    command.Parameters.AddWithValue("@Status", transaction.Status);
                    command.Parameters.AddWithValue("@CreatedAt", transaction.CreatedAt);
                    command.Parameters.AddWithValue("@RTSUniqueID", transaction.RTSUniqueID);
                    command.Parameters.AddWithValue("@TenderedAmount", transaction.TenderedAmount);
                    command.Parameters.AddWithValue("@TransactionAmount", transaction.TransactionAmount);
                    command.Parameters.AddWithValue("@Finalised", transaction.Finalised);
                    command.Parameters.AddWithValue("@StatusRequestCount", transaction.StatusRequestCount);
                    command.Parameters.AddWithValue("@Sold", transaction.Sold);
                    command.Parameters.AddWithValue("@DebitRecovery", transaction.DebitRecovery);
                    command.Parameters.AddWithValue("@CostOfUnits", transaction.CostOfUnits);
                    command.Parameters.AddWithValue("@VendtechTransactionId", transaction.TransactionId);
                    command.Parameters.AddWithValue("@RequestDate", transaction.RequestDate);
                    command.Parameters.AddWithValue("@CurrentDealerBalance", transaction.CurrentDealerBalance);
                    command.Parameters.AddWithValue("@TaxCharge", transaction.TaxCharge);
                    command.Parameters.AddWithValue("@Units", transaction.Units);

                    // Execute the SQL command
                    await command.ExecuteNonQueryAsync();
                }
            }

            return transaction;
        }

        public async Task<string> GenerateNewTransactionId()
        {
            string transactionId;

            string query = @"SELECT TOP 1 VendtechTransactionId 
                     FROM TransactionDetails 
                     ORDER BY TransactionDetailsId DESC";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    if (result != null && long.TryParse(result.ToString(), out long lastTransactionId))
                    {
                        transactionId = (lastTransactionId + 1).ToString();
                    }
                    else
                    {
                        transactionId = "1";
                    }
                }
            }

            return transactionId;
        }

    }
}
