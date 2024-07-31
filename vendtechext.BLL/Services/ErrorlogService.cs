using vendtechext.BLL.Interfaces;
using vendtechext.DAL;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class ErrorlogService : IErrorlogService
    {
        public readonly DataContext dataContext; 
        public ErrorlogService(DataContext dataContext)
        {
                this.dataContext = dataContext;
        }
        public void LogExceptionToDatabase(Exception exc, string clientkey)
        {
            ErrorLog errorObj = new ErrorLog();
            errorObj.Message = exc.Message;
            errorObj.StackTrace = exc.StackTrace;
            errorObj.InnerException = exc.InnerException == null ? "" : exc.InnerException.Message;
            errorObj.LoggedInDetails = $"Client: {clientkey} ";
            errorObj.LoggedAt = DateTime.UtcNow;
            errorObj.UserId = 0;
            dataContext.ErrorLogs.Add(errorObj);
            dataContext.SaveChanges();
        }
    }
}
