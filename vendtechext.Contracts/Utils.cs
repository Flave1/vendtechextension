using Newtonsoft.Json;
using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class Utils
    {
        public static string WriteResponseToFile(RTSResponse response, string transactionId)
        {
            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            File.WriteAllText($"{transactionId}.json", json);
            Console.WriteLine($"Response written to file {transactionId}.json");
            return json;
        }

        public static string formatDate(DateTime date)
        {
            return date.ToString("dd-MM-yyyy hh:mm");
        }

        public static bool IsAscending(string sortOrder) => sortOrder == "ASC";

        public static string FormatAmount(decimal? amt)
        {
            if (amt.ToString().Contains('.'))
            {
                var splitedAmt = amt.ToString().Split('.');
                var d = "." + splitedAmt[1];
                var result = amt == null ? "0" : string.Format("{0:N0}", Convert.ToDecimal(splitedAmt[0])) + "" + d;
                return result;
            }
            else
            {
                return amt == null ? "0" : string.Format("{0:N0}", amt) + "";
            }
        }

       
    }
}
