using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IDepositService
    {
        Task<APIResponse> ApproveDeposit(ApproveDepositRequest request);
        Task<APIResponse> CreateDeposit(DepositRequest request, Guid integratorid);
        Task<APIResponse> GetIntegratorDeposits(Guid integratorId);
        Task<APIResponse> GetPaymentTypes();
        Task<APIResponse> GetPendingDeposits();
        Task<APIResponse> GetWalletBalance(Guid integratorId, bool includeLastDeposit);
    }
}
