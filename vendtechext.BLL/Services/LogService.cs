using Newtonsoft.Json;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
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

        public void Log(LogType type, string message, dynamic data = null, string stackTrace = "")
        {
            using (var context = new DataContext())
            {
                var log = new Log
                {
                    LogType = (int)type,
                    Message = message,
                    Detail = JsonConvert.SerializeObject(data),
                    StackTrace = stackTrace,
                    Timestamp = DateTime.UtcNow,
                };
                context.Logs.Add(log);
                context.SaveChanges();
            }
        }
    }
}
