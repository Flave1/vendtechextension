using vendtechext.DAL;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Common
{
    public class Utils
    {
        public static string GetElectricityLastTrxId()
        {
            DataContext context = new DataContext();
            var lastRecord = context.ElectricityTrxLogs.OrderByDescending(d => d.ElectricityTrxLogsId).FirstOrDefault();
            var trId = lastRecord != null ? Convert.ToInt64(lastRecord.TransactionId) + 1 : 1;
            return trId.ToString();
        }

    }
}
