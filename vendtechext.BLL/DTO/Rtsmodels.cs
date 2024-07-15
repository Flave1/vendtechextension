﻿using static System.Runtime.InteropServices.JavaScript.JSType;

namespace vendtechext.BLL.DTO
{
    public class RTSRequestmodel
    {
        public RTSAuthModel Auth { get; set; }

        public string Request { get; set; }

        public List<object> Parameters { get; set; }
    }

    public partial class RTSAuthModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public partial class RTSResponse
    {
        public string Status { get; set; }

        public Content Content { get; set; } = new Content();

        public string[] ErrorLog { get; set; }

        public RTSRequestmodel RequestModel { get; set; } = new RTSRequestmodel();
    }

    public partial class Content
    {
        public DataResponse Data { get; set; } = new DataResponse();

        public string ProcessOption { get; set; }
    }

  
    public partial class DataResponse
    {
        public Datum[] Data { get; set; } = new Datum[0];

        public string DataName { get; set; }

        public string Error { get; set; }

        public long ErrorCode { get; set; }
    }

    public partial class Datum
    {
        public long Barcode { get; set; }

        public string DateAndTime { get; set; }

        public long DealerBalance { get; set; }

        public long Denomination { get; set; }

        public string ErrorMsg { get; set; }

        public long Id { get; set; }

        public string Instructions { get; set; }

        public string PinNumber { get; set; }

        public string PinNumber2 { get; set; }

        public string PinNumber3 { get; set; }

        public PowerHubVoucher PowerHubVoucher { get; set; } = new PowerHubVoucher();

        public string Provider { get; set; }

        public string SerialNumber { get; set; }

        public Tym2SellVoucher Tym2SellVoucher { get; set; } = new Tym2SellVoucher();

        public long VoucherProfit { get; set; }

        public string XmlResponse { get; set; }
    }


    public partial class PowerHubVoucher
    {
        public long AccountCredit { get; set; }

        public string AccountNumber { get; set; }

        public string CostOfUnits { get; set; }

        public string CustAccNo { get; set; }

        public string CustAddress { get; set; }

        public string CustCanVend { get; set; }

        public string CustContactNo { get; set; }

        public string CustDaysLastPurchase { get; set; }

        public string CustLocalRef { get; set; }

        public string CustMsno { get; set; }

        public string CustMinVendAmt { get; set; }

        public string CustName { get; set; }

        public string Customer { get; set; }

        public long DebtRecoveryAmt { get; set; }
        public long DebtRecoveryBalance { get; set; }

        public string MeterNumber { get; set; }

        public string PayAccDesc { get; set; }

        public string PayAccNo { get; set; }

        public string PayAmount { get; set; }

        public string PayBalance { get; set; }

        public string PayReceiptNo { get; set; }

        public string Pin1 { get; set; }

        public string Pin2 { get; set; }

        public string Pin3 { get; set; }

        public string RtsUniqueId { get; set; }

        public string ReceiptNumber { get; set; }

        public string Sgc { get; set; }

        public string ServiceCharge { get; set; }

        public string Tariff { get; set; }

        public string TaxCharge { get; set; }

        public string TenderedAmount { get; set; }

        public string TransactionAmount { get; set; }

        public string Units { get; set; }

        public long VatNumber { get; set; }
    }

    public partial class Tym2SellVoucher
    {
        public string Account { get; set; }

        public string ClientId { get; set; }

        public string CostOfUnits { get; set; }

        public string Customer { get; set; }

        public long GovermentLevy { get; set; }

        public bool KeyChangeDetected { get; set; }

        public string KeyChangeToken1 { get; set; }

        public string KeyChangeToken2 { get; set; }

        public string ReceiptNumber { get; set; }

        public long StandingCharge { get; set; }

        public string StsMeter { get; set; }

        public string TenderedAmount { get; set; }

        public string Units { get; set; }

        public string Vat { get; set; }

        public string VatNo { get; set; }

        public bool VoucherTextDecodeFailed { get; set; }
    }
}
