using vendtechext.BLL;
using vendtechext.BLL.DTO;
using vendtechext.DAL.Models;

namespace signalrserver.Models.DTO
{
    public class SuccessResponse
    {
        //public string IntegratorId { get; set; }
        public string TransactionId { get; set; }
        public DateTime RequestDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? CurrentDealerBalance { get; set; }
        public string MeterNumber { get; set; }
        public int TransactionStatus { get; set; }
        public string MeterToken1 { get; set; }
        public string MeterToken2 { get; set; }
        public string MeterToken3 { get; set; }
        public string AccountNumber { get; set; }
        public string Customer { get; set; }
        public string RTSUniqueID { get; set; }
        public string ReceiptNumber { get; set; }
        public string ServiceCharge { get; set; }
        public string Tariff { get; set; }
        public string TaxCharge { get; set; }
        public string CostOfUnits { get; set; }
        public string Units { get; set; }
        public string DebitRecovery { get; set; }
        public string SerialNumber { get; set; }
        public string CustomerAddress { get; set; }
        public string VProvider { get; set; }
        public bool Finalised { get; set; }
        public int StatusRequestCount { get; set; }
        public bool Sold { get; set; }
        public string VoucherSerialNumber { get; set; }
        public string VendStatusDescription { get; set; }
        public string VendStatus { get; set; }
        //public string Request { get; set; }
        //public string Response { get; set; }
        public string DateAndTimeSold { get; set; }
        public string DateAndTimeFinalised { get; set; }
        public string DateAndTimeLinked { get; set; }
        public int QueryStatusCount { get; set; }
        public SuccessResponse(RTSResponse x)
        {
            var response_data = x.Content.Data.Data.FirstOrDefault();
            CurrentDealerBalance = response_data.DealerBalance;
            CostOfUnits = response_data.PowerHubVoucher.CostOfUnits;
            MeterToken1 = response_data?.PowerHubVoucher.Pin1?.ToString() ?? string.Empty;
            MeterToken2 = response_data?.PowerHubVoucher?.Pin2?.ToString() ?? string.Empty;
            MeterToken3 = response_data?.PowerHubVoucher?.Pin3?.ToString() ?? string.Empty;
            AccountNumber = response_data.PowerHubVoucher?.AccountNumber ?? string.Empty;
            Customer = response_data.PowerHubVoucher?.Customer ?? string.Empty;
            ReceiptNumber = response_data?.PowerHubVoucher.ReceiptNumber ?? string.Empty;
            SerialNumber = response_data?.SerialNumber ?? string.Empty;
            RTSUniqueID = response_data.PowerHubVoucher.RtsUniqueId;
            ServiceCharge = response_data?.PowerHubVoucher?.ServiceCharge;
            Tariff = response_data.PowerHubVoucher?.Tariff;
            TaxCharge = response_data?.PowerHubVoucher?.TaxCharge;
            Units = response_data?.PowerHubVoucher?.Units;
            VProvider = "";
            CustomerAddress = response_data?.PowerHubVoucher?.CustAddress;
            Finalised = true;
            VProvider = response_data.Provider;
            StatusRequestCount = 0;
            Sold = true;
            VoucherSerialNumber = response_data?.SerialNumber;
        }
        public SuccessResponse UpdateResponse(Transaction x)
        {
            TransactionId = x.TerminalId;
            RequestDate = x.CreatedAt;
            Amount = x.Amount;
            return this;
        }
    }
    public class FailedResponse
    {
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public FailedResponse(RTSErorResponse x)
        {
            ErrorDetail = x.Stack[0].Detail;
            ErrorMessage = x.SystemError;
        }
    }
    public class ExecutionResult
    {
        public string Status { get; set; }
        public SuccessResponse SuccessResponse { get; set; }
        public FailedResponse FailedResponse { get; set; }
        public string Request;
        public string Response;
        public ExecutionResult(RTSResponse x)
        {
            SuccessResponse = new SuccessResponse(x);
        }
        public ExecutionResult(RTSErorResponse x)
        {
            FailedResponse = new FailedResponse(x);
        }

        public ExecutionResult InitializeRequestAndResponse(RequestExecutionContext executionResult)
        {
            Request = executionResult.requestAsString;
            Response = executionResult.responseAsString;
            return this;
        }
    }

}
