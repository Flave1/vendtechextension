using System;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Common
{
    public static class UniqueIDGenerator
    {
        private static readonly object _salesLock = new object();
        private static readonly object _depositLock = new object();
        private static Random _random = new Random();

        public static string NewSaleTransactionId()
        {
            lock (_salesLock) // Ensure this is thread-safe, especially if accessed concurrently
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

        public static string NewDepositTransactionId()
        {
            lock (_salesLock) // Ensure this is thread-safe, especially if accessed concurrently
            {
                using (var context = new DataContext())
                {
                    try
                    {
                        // Fetch the last transaction ordered by VendtechTransactionId (converted to long)
                        var lastRecord = context.Deposits
                         .OrderByDescending(t => Convert.ToInt64(t.TransactionId)) // Casting to long
                         .FirstOrDefault();

                        long transactionId;

                        if (lastRecord != null && long.TryParse(lastRecord.TransactionId, out transactionId))
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

        public static string GenerateAccountNumber(string branchCode)
        {
            // Validate branch Code
            if (branchCode.Length != 3 || !branchCode.All(char.IsDigit))
            {
                throw new ArgumentException("Branch Code must be 3 digits.");
            }

            // Generate a unique customer identifier (6 random digits)
            string customerId = GenerateCustomerId();

            // Combine branch Code and customer ID to create the initial account number
            string initialAccountNumber = branchCode + customerId;

            // Calculate the check digit
            string checkDigit = CalculateCheckDigit(initialAccountNumber);

            // Complete account number (10 digits)
            string accountNumber = initialAccountNumber + checkDigit;

            // Check for uniqueness and store it
            using (DataContext context = new DataContext())
            {
                if (context.Wallets.Any(d => d.WALLET_ID.Contains(accountNumber)))
                {
                    return GenerateAccountNumber(branchCode);
                }
            }
            // Store the account number
            return accountNumber;
        }

        private static string GenerateCustomerId()
        {
            // Generate 6 random digits
            return _random.Next(100000, 999999).ToString();
        }

        private static string CalculateCheckDigit(string accountNumber)
        {
            int sum = 0;
            for (int i = 0; i < accountNumber.Length; i++)
            {
                int digit = int.Parse(accountNumber[i].ToString());
                // Weighting based on position (simple example)
                sum += digit * (i + 1);
            }
            int checkDigit = sum % 10; // Check digit as the last digit
            return checkDigit.ToString();
        }
    }
}
