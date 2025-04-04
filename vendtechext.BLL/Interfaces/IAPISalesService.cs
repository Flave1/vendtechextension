﻿using vendtechext.Contracts;

namespace vendtechext.BLL.Interfaces
{
    public interface IAPISalesService
    {
        Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, Guid integratorid, string integratorName);
        Task<APIResponse> PurchaseElectricityForSandbox(ElectricitySaleRequest request, Guid integratorid, string integratorName);
        Task<APIResponse> QuerySalesStatus(SaleStatusRequest request, Guid integratorid, string integratorName);
        Task<APIResponse> QuerySalesStatusForSandbox(SaleStatusRequest request, Guid integratorid, string integratorName);
    }
}
