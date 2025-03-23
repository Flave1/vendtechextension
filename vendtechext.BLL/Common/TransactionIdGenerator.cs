using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using vendtechext.DAL.Common;
using vendtechext.Helper;

namespace vendtechext.BLL.Common
{
    public class TransactionIdGenerator
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "EXTTransactionIds";
        private readonly IConfiguration _configuration;
        private readonly LogService _logService;

        public TransactionIdGenerator(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            LogService logService
        )
        {
            _cache = memoryCache;
            _configuration = configuration;
            _logService = logService;
        }

        public bool TransactionIdExist(long transactionId)
        {
            if (_cache.TryGetValue(CacheKey, out List<long> ids))
            {
                _logService.Log(
                    LogType.Infor,
                    $"Checking TransactionIdExist for {transactionId}",
                    string.Join(", ", ids)
                );

                return ids.Contains(transactionId);
            }
            return false;
        }

        public void SetTransactionId(long transactionId)
        {
            List<long> ids;

            if (!_cache.TryGetValue(CacheKey, out ids))
            {
                ids = new List<long>();
            }

            ids.Add(transactionId);

            // Set or refresh the cache
            _cache.Set(CacheKey, ids, TimeSpan.FromMinutes(10));
        }

        public async Task<string> GenerateNewTransactionId()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                long transactionId = 0;
                string query =
                    @"SELECT TOP 1 VendtechTransactionID FROM Transactions ORDER BY CreatedAt DESC";

                await using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync().ConfigureAwait(false);

                    await using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                        if (
                            result != null
                            && long.TryParse(result.ToString(), out long lastTransactionId)
                        )
                        {
                            transactionId = lastTransactionId;
                            do
                            {
                                transactionId = transactionId + 1;
                            } while (TransactionIdExist(transactionId));

                            SetTransactionId(transactionId);
                        }
                        else
                        {
                            transactionId = 1;
                        }
                    }
                }

                return transactionId.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on GenerateNewTransactionId: {ex.Message}");
                throw;
            }
        }
    }
}
