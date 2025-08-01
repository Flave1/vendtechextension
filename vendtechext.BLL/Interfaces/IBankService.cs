using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IBankService
    {
        Task<APIResponse> CreateBankAsync(CreateBankRequest request);
        Task<APIResponse> UpdateBankAsync(UpdateBankRequest request);
        Task<APIResponse> DeleteBankAsync(int id);
        Task<APIResponse> GetBankByIdAsync(int id);
        Task<APIResponse> GetAllBanksAsync();
        Task<IList<SelectItem>> GetBankSelect();
    }
} 