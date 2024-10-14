using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IElectricitySalesService
    {
        Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, Guid integratorid, string integratorName);
        Task<APIResponse> QuerySalesStatus(SaleStatusRequest request, Guid integratorid, string integratorName);
    }
}
