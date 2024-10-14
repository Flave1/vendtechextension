using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IIntegratorService
    {
        Task<BusinessUserDTO> GetIntegrator(string apiKey);
        Task CreateBusinessAccount(BusinessUserCommandDTO model);
        Task UpdateBusinessAccount(BusinessUserDTO model);
        Task DeleteBusinessAccount(Guid Id);
        Task<(string, string)> GetIntegratorIdAndName(string apiKey);
    }
}
