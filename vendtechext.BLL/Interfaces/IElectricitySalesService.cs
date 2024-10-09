using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IElectricitySalesService
    {
        Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, string integratorid, string integratorName);
        Task<APIResponse> QuerySalesStatus(SaleStatusRequest request, string integratorid, string integratorName);
    }
}
