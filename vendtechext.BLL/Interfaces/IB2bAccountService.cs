using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IB2bAccountService
    {
        Task<BusinessUserDTO> GetIntegrator(string apiKey);
        Task CreateBusinessAccount(BusinessUserCommandDTO model);
        Task UpdateBusinessAccount(BusinessUserDTO model);
        Task DeleteBusinessAccount(Guid Id);
        Task DeleteBusinessAccount(string email);
        Task<(string, string)> GetIntegratorIdAndName(string apiKey);
    }
}
