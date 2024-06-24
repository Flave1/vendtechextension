using vendtechext.DAL;

namespace vendtechext.BLL.Interfaces
{
    public interface IB2bAccountService
    {
        bool ValidateUser(string apiKey, string clientKey);
    }
}
