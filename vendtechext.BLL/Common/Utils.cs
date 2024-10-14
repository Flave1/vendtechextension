using Newtonsoft.Json;
using vendtechext.Contracts;

namespace vendtechext.BLL.Common
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
    }
}
