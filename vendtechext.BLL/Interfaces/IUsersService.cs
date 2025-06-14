using vendtechext.Contracts;
using vendtechext.DAL.Common;

namespace vendtechext.BLL.Interfaces
{
    public interface IUsersService
    {
        Task<APIResponse> CreateAgencyAccount(AgencyAccount model);
        Task<APIResponse> CreateVendorAccount(VendorAccount model);
        Task<APIResponse> GetAgencyAccountById(string id);
        Task<APIResponse> UpdateAgencyAccount(string userid, AgencyAccount model);
        Task<APIResponse> UpdateVendorAccount(string userid, VendorAccount model);
        Task<APIResponse> GetVendorAccountById(string id);
        Task<APIResponse> GetUserAccounts(UserType type);
    }
}
