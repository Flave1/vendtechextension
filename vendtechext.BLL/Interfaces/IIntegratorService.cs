using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IIntegratorService
    {
        Task<BusinessUserDTO> GetIntegrator(string apiKey);
        Task<APIResponse> CreateBusinessAccount(BusinessUserCommandDTO model);
        Task<APIResponse> UpdateBusinessAccount(BusinessUserDTO model);
        Task<APIResponse> DeleteBusinessAccount(Guid Id);
        Task<(string, string)> GetIntegratorIdAndName(string apiKey);
        Task<APIResponse> GetIntegrators(PaginatedSearchRequest req);
        Task<APIResponse> GetIntegrator(Guid id);
        Task<APIResponse> EnableDisable(EnableIntegrator model);
    }
}
