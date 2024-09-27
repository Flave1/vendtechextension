using vendtechext.DAL.Common;

namespace vendtechext.BLL.Interfaces
{
    public interface ILogService
    {
        void Log(LogType type, string message, dynamic data = null, string stackTrace = "");
    }
}
