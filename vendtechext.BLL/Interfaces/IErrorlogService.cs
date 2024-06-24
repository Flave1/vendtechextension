namespace vendtechext.BLL.Interfaces
{
    public interface IErrorlogService
    {
        void LogExceptionToDatabase(Exception exc, string clientkey);
    }
}
