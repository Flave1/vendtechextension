using vendtechext.DAL.Models;

namespace vendtechext.BLL.Common
{
    public static class UniqueIDGenerator
    {
        private static readonly object _lock = new object(); // Ensure thread-safety

        public static string NewTransactionId()
        {
            lock (_lock) // Ensure this is thread-safe, especially if accessed concurrently
            {
                using (var context = new DataContext())
                {
                    try
                    {
                        // Fetch the last transaction ordered by VendtechTransactionId (converted to long)
                        var lastRecord = context.Transactions
                         .OrderByDescending(t => Convert.ToInt64(t.VendtechTransactionID)) // Casting to long
                         .FirstOrDefault();

                        long transactionId;

                        if (lastRecord != null && long.TryParse(lastRecord.VendtechTransactionID, out transactionId))
                        {
                            // Increment the transactionId from the last record
                            transactionId += 1;
                        }
                        else
                        {
                            // Start from 1 if no records exist
                            transactionId = 1;
                        }

                        // Update the _previousId if needed (in case of any in-memory checks)
                        return transactionId.ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Transaction ID generation failed at: " + DateTime.Now);
                        Console.WriteLine("Error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
    }
}
