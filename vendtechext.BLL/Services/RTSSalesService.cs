using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using vendtechext.BLL.Common;
using vendtechext.BLL.Configurations;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class RTSSalesService: IRTSSalesService
    {
        private readonly DataContext dtcxt;
        private readonly IHttpRequestService http;
        private readonly RTSInformation rts;
        public RTSSalesService(DataContext dtcxt, IHttpRequestService http, IOptions<RTSInformation> rts)
        {
            this.rts = rts.Value;
            this.dtcxt = dtcxt;
            this.http = http;
        }

        public async Task<HttpResponseMessage> PurchaseElectricity(RTSRequestmodel request)
        {
            var clientInfor = dtcxt.B2bUserAccesses
                .Include(d => d.User)
                .FirstOrDefault(d => request.Auth.UserName.Equals(d.Clientkey) && request.Auth.Password.Equals(d.Apikey));

            if(clientInfor == null)
            {
                throw new Exception("Unable to validate information");
            }

            ElectricityTrxLog trxlog = await CreateRecordBeforeVend(clientInfor, request);
            var httpreq = await http.SendPostAsync(rts.ProductionUrl, request);

            httpreq.EnsureSuccessStatusCode();
            string stringResult = await httpreq.Content.ReadAsStringAsync();
            //Deserialize here
            //if error, map to error model
            //save a copy if success
            //save a copy if failed and return to user
            //if (vendResponse.Content.Data.Error == "Error")
            //{
            //    throw new ArgumentException(vendResponse.Content.Data.Error);
            //}

            return httpreq;
        }

        private async Task<ElectricityTrxLog> CreateRecordBeforeVend(B2bUserAccess client, RTSRequestmodel request)
        {
            var trans = new ElectricityTrxLog();
            trans.PlatFormId = (int)PlatformTypeEnum.ELECTRICITY;
            trans.UserId = client.UserId;
            trans.MeterNumber = request.Parameters[5].ToString();
            trans.MeterToken1 = "";
            trans.TransactionId = Utils.GetElectricityLastTrxId();
            trans.IsDeleted = false;
            trans.Status = (int)TransactionStatus.Pending;
            trans.CreatedAt = DateTime.UtcNow;
            trans.AccountNumber = "";
            trans.Customer = "";
            trans.ReceiptNumber = "";
            trans.RequestDate = DateTime.UtcNow;
            trans.SerialNumber = "";
            trans.ServiceCharge = "";
            trans.Tariff = "";
            trans.TaxCharge = "";
            trans.TenderedAmount = Convert.ToDecimal(request.Parameters[4]);
            trans.TransactionAmount = Convert.ToDecimal(request.Parameters[4]);
            trans.Units = "";
            trans.Vprovider = "";
            trans.Finalised = false;
            trans.StatusRequestCount = 0;
            trans.Sold = false;
            trans.DateAndTimeSold = "";
            trans.DateAndTimeFinalised = "";
            trans.DateAndTimeLinked = "";
            trans.VoucherSerialNumber = "";
            trans.VendStatus = "";
            trans.VendStatusDescription = "";
            trans.StatusResponse = "";
            trans.DebitRecovery = "0";
            trans.CostOfUnits = "0";

            dtcxt.ElectricityTrxLogs.Add(trans);
            await dtcxt.SaveChangesAsync();

            return trans;
        }

    }
}
