using Newtonsoft.Json;
using vendtechext.BLL.DTO;
using vendtechext.DAL;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Common
{
    public class Utils
    {
        //public static string GetElectricityLastTrxId()
        //{
        //    DataContext context = new DataContext();
        //    var lastRecord = context.ElectricityTrxLogs.OrderByDescending(d => d.ElectricityTrxLogsId).FirstOrDefault();
        //    var trId = lastRecord != null ? Convert.ToInt64(lastRecord.TransactionId) + 1 : 1;
        //    return trId.ToString();
        //}


        public static string NewTransactionId()
        {
            using (var context = new DataContext())
            {
                //System.Data.IsolationLevel.Serializable
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var lastRecord = context.Transactions
                            .OrderByDescending(d => d.TransactionId)
                            .FirstOrDefault();

                        var trId = lastRecord != null ? Convert.ToInt64(lastRecord.TransactionId) + 1 : 1;
                        context.SaveChanges();
                        transaction.Commit();
                        return trId.ToString();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Transaction rolled back at: " + DateTime.Now);
                        Console.WriteLine("Error: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public static string WriteResponseToFile(RTSResponse response, string transactionId)
        {
            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            File.WriteAllText($"{transactionId}.json", json);
            Console.WriteLine($"Response written to file {transactionId}.json");
            return json;
        }
    }
}
