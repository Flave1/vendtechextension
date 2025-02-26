using Newtonsoft.Json;

namespace vendtechext.Contracts
{
    public class Utils
    {
        public static string WriteResponseToFile(RTSResponse response, string transactionId)
        {
            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            File.WriteAllText($"{transactionId}.json", json);
            Console.WriteLine($"response written to file {transactionId}.json");
            return json;
        }

        public static string formatDate(DateTime date)
        {
            return date.ToString("dd-MM-yyyy hh:mm");
        }

        public static bool IsAscending(string sortOrder) => sortOrder == "ASC";

        public static string FormatAmount(decimal? amt)
        {
            if(amt != null && amt.Value != 0)
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
            else
            {
                return "0";
            }
           
        }

       
    }
}
