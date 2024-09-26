using Newtonsoft.Json;
using vendtechext.BLL.DTO;
using vendtechext.DAL;
using vendtechext.DAL.Models;

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
    }
}
