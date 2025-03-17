using Microsoft.Data.SqlClient;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using vendtechext.BLL.Interfaces;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using Xunit;

namespace vendtechext.TEST.Sales
{
    public class PurchaseElectricitySalesTest
    {
        private readonly HttpClient _client;
        private readonly Mock<IAPISalesService> _mockSalesService;
        const string transactionId = "274867";//274733
        const string devApikey = "FCcHkRm7bBTaJkjgFyL6C2FH6RSGy6ff0YX3zK1kok87R+HL4blEj+PygevBefS0";
        const string liveApikey = "e+KZgZZl1GZcLUHQkZ2lqQmWwAHBQvyQZ99ChmNOd4+HCoVqRm/trmKOztwiv7LB";
        const string meternumber = "98000142897";

        private readonly string _connectionString;
        public PurchaseElectricitySalesTest()
        {
            TestServerFixture testServer = new TestServerFixture();
            _client = testServer.Client;
            _mockSalesService = new Mock<IAPISalesService>();
            _connectionString = "Server=92.205.181.48;Database=VENDTECH_MAIN;User Id=vendtech_main;Password=85236580@Ve;MultipleActiveResultSets=True;TrustServerCertificate=true;";
        }

        [Theory]
        [InlineData(liveApikey, 40, meternumber, 2002)]
        public async Task Test_for_successful_response(
            string apiKey,
            decimal amount,
            string meterNumber,
            int expectedStatusCode)

        {

            dynamic transaction = await CreateRecordBeforeVend(meterNumber, amount);
            // Arrange
            var requestModel = new ElectricitySaleRequest
            {
                TransactionId = transaction.TransactionId,
                MeterNumber = meterNumber,
                Amount = amount,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            content.Headers.Add("X-Api-Key", apiKey);
            // Act
            var response = await _client.PostAsync("/sales/v1/buy", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            APIResponse result = JsonConvert.DeserializeObject<APIResponse>(responseString);
            Assert.Equal(expectedStatusCode, Convert.ToInt16(result.result.code));
            // Additional assertions to validate the response
        }


        [Theory]
        [InlineData(liveApikey, "330878", HttpStatusCode.OK)]
        public async Task Test_for_successful_query(
           string apiKey,
           string transactionId,
           HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var requestModel = new SaleStatusRequest
            {
                TransactionId = transactionId,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            content.Headers.Add("X-Api-Key", apiKey);
            // Act
            var response = await _client.PostAsync("/sales/v1/status", content);

            // Assert
            //response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            APIResponse result = JsonConvert.DeserializeObject<APIResponse>(responseString);

            Assert.NotNull(result);
            Assert.Equal(expectedStatusCode, response.StatusCode);
            // Additional assertions to validate the response
        }

        public async Task<string> GenerateNewTransactionId()
        {
            try
            {
                string transactionId;

                string query = @"SELECT TOP 1 TransactionId 
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
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<dynamic> CreateRecordBeforeVend(string meterNumber, decimal amount)
        {
            try
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
                    PaymentStatus = 1,
                    VoucherSerialNumber = "",
                };

                // Build the SQL query
                var query = @"INSERT INTO TransactionDetails 
                     (PlatFormId, UserId, MeterNumber1, POSId, Amount, 
                      IsDeleted, status, CreatedAt, RTSUniqueID, TenderedAmount, 
                      TransactionAmount, Finalised, StatusRequestCount, Sold, DebitRecovery, 
                      CostOfUnits, TransactionId, RequestDate, CurrentDealerBalance, TaxCharge, Units, PaymentStatus, VoucherSerialNumber)
                      VALUES 
                      (@PlatFormId, @UserId, @MeterNumber1, @POSId, @Amount,
                       @IsDeleted, @status, @CreatedAt, @RTSUniqueID, @TenderedAmount, 
                       @TransactionAmount, @Finalised, @StatusRequestCount, @Sold, @DebitRecovery, 
                       @CostOfUnits, @TransactionId, @RequestDate, @CurrentDealerBalance, @TaxCharge, @Units, @PaymentStatus, @VoucherSerialNumber)";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@PlatFormId", transaction.PlatFormId);
                        command.Parameters.AddWithValue("@UserId", transaction.UserId);
                        command.Parameters.AddWithValue("@MeterNumber1", transaction.MeterNumber1);
                        command.Parameters.AddWithValue("@POSId", transaction.POSId);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@IsDeleted", transaction.IsDeleted);
                        command.Parameters.AddWithValue("@status", transaction.Status);
                        command.Parameters.AddWithValue("@CreatedAt", transaction.CreatedAt);
                        command.Parameters.AddWithValue("@RTSUniqueID", transaction.RTSUniqueID);
                        command.Parameters.AddWithValue("@TenderedAmount", transaction.TenderedAmount);
                        command.Parameters.AddWithValue("@TransactionAmount", transaction.TransactionAmount);
                        command.Parameters.AddWithValue("@Finalised", transaction.Finalised);
                        command.Parameters.AddWithValue("@StatusRequestCount", transaction.StatusRequestCount);
                        command.Parameters.AddWithValue("@Sold", transaction.Sold);
                        command.Parameters.AddWithValue("@DebitRecovery", transaction.DebitRecovery);
                        command.Parameters.AddWithValue("@CostOfUnits", transaction.CostOfUnits);
                        command.Parameters.AddWithValue("@TransactionId", transaction.TransactionId);
                        command.Parameters.AddWithValue("@RequestDate", transaction.RequestDate);
                        command.Parameters.AddWithValue("@CurrentDealerBalance", transaction.CurrentDealerBalance);
                        command.Parameters.AddWithValue("@TaxCharge", transaction.TaxCharge);
                        command.Parameters.AddWithValue("@Units", transaction.Units);
                        command.Parameters.AddWithValue("@VoucherSerialNumber", transaction.VoucherSerialNumber);
                        command.Parameters.AddWithValue("@PaymentStatus", transaction.PaymentStatus);

                        // Execute the SQL command
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return transaction;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
