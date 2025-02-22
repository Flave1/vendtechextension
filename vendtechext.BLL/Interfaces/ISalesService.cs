using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface ISalesService
    {
        Task<APIResponse> GetSalesReportAsync(PaginatedSearchRequest request);
        Task<List<TransactionExportDto>> GetSalesReportForExportAsync(PaginatedSearchRequest req);
        Task<APIResponse> GetSingleTransactionAsync(SingleTransation req);
    }
}
