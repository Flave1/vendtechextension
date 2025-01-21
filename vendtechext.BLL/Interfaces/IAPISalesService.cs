using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IAPISalesService
    {
        Task<APIResponse> PurchaseElectricityForSandbox(ElectricitySaleRequest request, Guid integratorid, string integratorName);
        Task<APIResponse> QuerySalesStatusForSandbox(SaleStatusRequest request, Guid integratorid, string integratorName);
    }
}
