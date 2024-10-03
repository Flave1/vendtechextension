using vendtechext.DAL.Common;

namespace vendtechext.Helper
{
    public interface ILogService
    {
        void Log(LogType type, string message, dynamic data = null, string stackTrace = "");
    }
}
