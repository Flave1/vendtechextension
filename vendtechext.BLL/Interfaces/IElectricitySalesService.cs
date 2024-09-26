using vendtechext.BLL.DTO;

namespace vendtechext.BLL.Interfaces
{
    public interface IElectricitySalesService
    {
        Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, string integratorid);
    }
}
