using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IDepositService
    {
        Task<APIResponse> ApproveDeposit(ApproveDepositRequest request);
        Task<APIResponse> CreateDeposit(DepositRequest request, Guid integratorid);
        Task<APIResponse> GetIntegratorDeposits(PaginatedSearchRequest request);
        Task<APIResponse> GetPaymentTypes();
        Task<APIResponse> GetPendingDeposits(PaginatedSearchRequest req);
        Task<List<DepositExcelDto>> GetDepositReportForExportAsync(PaginatedSearchRequest req);
        Task<APIResponse> GetWalletBalance(Guid integratorId, bool includeLastDeposit);
        APIResponse GetTodaysTransaction(Guid integratorId);
        APIResponse GetAdminTodaysTransaction();
        Task<APIResponse> GetAdminBalance();
    }
}
