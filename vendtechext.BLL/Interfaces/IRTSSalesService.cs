using Newtonsoft.Json;
using vendtechext.BLL.DTO;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Interfaces
{
    public interface IRTSSalesService
    {
        Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, string integratorid);
    }
}
