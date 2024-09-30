using vendtechext.BLL.DTO;
using vendtechext.DAL;

namespace vendtechext.BLL.Interfaces
{
    public interface IB2bAccountService
    {
        Task<BusinessUserQueryDTO> GetIntegrator(string apiKey);
        Task CreateBusinessAccount(BusinessUserCommandDTO model);
        Task UpdateBusinessAccount(BusinessUserCommandDTO model);
        Task DeleteBusinessAccount(Guid Id);
        Task DeleteBusinessAccount(string email);
        Task<(string, string)> GetIntegratorIdAndName(string apiKey);
    }
}
