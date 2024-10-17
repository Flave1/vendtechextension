using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface ISalesService
    {
        Task<APIResponse> GetSalesReportAsync(PaginatedSearchRequest request);
    }
}
