using Newtonsoft.Json;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.Helper
{
    public class LogService : ILogService
    {
        public readonly DataContext _dataContext;
        public LogService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        void ILogService.Log(LogType type, string message, dynamic data, string stackTrace)
        {
            var log = new Log
            {
                LogType = (int)type,
                Message = message,
                Detail = JsonConvert.SerializeObject(data),
                StackTrace = stackTrace,
                Timestamp = DateTime.UtcNow,
            };
            _dataContext.Logs.Add(log);
            _dataContext.SaveChanges();
        }

    }
}
