using Newtonsoft.Json;
using vendtechext.DAL.Models;

namespace vendtechext.Contracts
{
    public class SuccessResponse
    {
        public string TransactionId { get; set; }
        public DateTime RequestDate { get; set; }
        public decimal Amount { get; set; }
        public string MeterNumber { get; set; }
        public int TransactionStatus { get; set; }
        public string VendtechTransactionId { get; set; }
        public string WalleBalance { get; set; }
        public Voucher Voucher { get; set; } = new Voucher();

        public SuccessResponse(RTSResponse x)
        {
            var response_data = x.Content.Data.Data.FirstOrDefault();
            Voucher.CostOfUnits = response_data.PowerHubVoucher.CostOfUnits;
            Voucher.MeterToken1 = response_data?.PowerHubVoucher.Pin1?.ToString() ?? string.Empty;
            Voucher.MeterToken2 = response_data?.PowerHubVoucher?.Pin2?.ToString() ?? string.Empty;
            Voucher.MeterToken3 = response_data?.PowerHubVoucher?.Pin3?.ToString() ?? string.Empty;
            Voucher.AccountNumber = response_data.PowerHubVoucher?.AccountNumber ?? string.Empty;
            Voucher.Customer = response_data.PowerHubVoucher?.Customer ?? string.Empty;
            Voucher.ReceiptNumber = response_data?.PowerHubVoucher.ReceiptNumber ?? string.Empty;
            Voucher.ServiceCharge = response_data?.PowerHubVoucher?.ServiceCharge;
            Voucher.Tariff = response_data.PowerHubVoucher?.Tariff;
            Voucher.TaxCharge = response_data?.PowerHubVoucher?.TaxCharge;
            Voucher.Units = response_data?.PowerHubVoucher?.Units;
            Voucher.CustomerAddress = response_data?.PowerHubVoucher?.CustAddress;
            Voucher.StatusRequestCount = 0;
            MeterNumber = response_data.PowerHubVoucher.MeterNumber;
            Voucher.VoucherSerialNumber = response_data?.SerialNumber;
            Voucher.Denomination = response_data?.Denomination;
            Voucher.RTSUniqueID = response_data?.PowerHubVoucher?.RtsUniqueId;
            Voucher.SellerReturnedBalance = response_data?.DealerBalance;
        }
        public SuccessResponse(RTSStatusResponse x)
        {
            var response_data = x.Content;

            Voucher.MeterToken1 = response_data?.VoucherPin?.ToString() ?? string.Empty;
            Voucher.MeterToken2 = response_data?.VoucherPin2?.ToString() ?? string.Empty;
            Voucher.MeterToken3 = response_data?.VoucherPin3?.ToString() ?? string.Empty;
            Voucher.AccountNumber = response_data.CustomerAccNo ?? string.Empty;
            Voucher.Customer = response_data.Customer ?? string.Empty;
            Voucher.ServiceCharge = response_data?.ServiceCharge;
            Voucher.Tariff = response_data.Tariff;
            Voucher.TaxCharge = response_data?.TaxCharge;
            Voucher.Units = response_data?.Units;
            Voucher.StatusRequestCount = 0;
            Voucher.VoucherSerialNumber = response_data?.SerialNumber;
            Voucher.RTSUniqueID = response_data?.RTSUniqueID;
            Voucher.Denomination = Convert.ToInt64(response_data?.Denomination);
            MeterNumber = response_data.MeterNumber;
        }
        public SuccessResponse UpdateResponse(Transaction x)
        {
            TransactionId = x.TransactionUniqueId;
            RequestDate = x.CreatedAt;
            Amount = x.Amount;
            VendtechTransactionId = x.VendtechTransactionID;
            WalleBalance = Utils.FormatAmount(x.BalanceAfter);
            return this;
        }
    }
    public class Voucher
    {
        public string MeterToken1 { get; set; }
        public string MeterToken2 { get; set; }
        public string MeterToken3 { get; set; }
        public string AccountNumber { get; set; }
        public string Customer { get; set; }
        public string ReceiptNumber { get; set; }
        public string ServiceCharge { get; set; }
        public string Tariff { get; set; }
        public string TaxCharge { get; set; }
        public string CostOfUnits { get; set; }
        public string Units { get; set; }
        public string DebitRecovery { get; set; }
        public string CustomerAddress { get; set; }
        public int StatusRequestCount { get; set; }
        public string VoucherSerialNumber { get; set; }
        public string VendStatusDescription { get; set; }
        public decimal DealerBalance { get; set; }
        public string RTSUniqueID { get; set; }
        public string Provider { get; set; } = "EDSA";
        public long? Denomination { get; set; }
        public decimal? SellerReturnedBalance { get; set; }

    }
    public class FailedResponse
    {
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public FailedResponse()
        {
                
        }
        public FailedResponse(string Detail, string SystemError)
        {
            ErrorDetail = Detail;
            ErrorMessage = SystemError;
        }
        public FailedResponse(RTSStatusResponse x)
        {
            ErrorDetail = x.Content.StatusDescription;
            ErrorMessage = x.Status;
        }
    }
    public class ExecutionResult
    {
        public string status { get; set; }
        public int code { get; set; }
        public SuccessResponse successResponse { get; set; }
        public FailedResponse failedResponse { get; set; }
        public string request;
        public string response;
        public string receivedFrom;
        public ExecutionResult(RTSResponse x)
        {
            successResponse = new SuccessResponse(x);
        }
        public ExecutionResult(Transaction transaction, string receivedFrom)
        {
            if (transaction.Response == null)
            {
                failedResponse = new FailedResponse("Transaction in-valid", "Invalid transaction");
                return;
            }
            if (!transaction.Finalized)
            {
                RTSErorResponse x = JsonConvert.DeserializeObject<RTSErorResponse>(transaction.Response);
                failedResponse = new FailedResponse(x.Stack[0].Detail, x.SystemError);
                return;
            }
            if (receivedFrom == "rts_init")
            {
                RTSResponse x = JsonConvert.DeserializeObject<RTSResponse>(transaction.Response);
                successResponse = new SuccessResponse(x);
                successResponse.UpdateResponse(transaction);
            }
            else if (receivedFrom == "rts_status")
            {
                RTSStatusResponse x = JsonConvert.DeserializeObject<RTSStatusResponse>(transaction.Response);
                successResponse = new SuccessResponse(x);
                successResponse.UpdateResponse(transaction);
            }
        }
        public ExecutionResult(RTSErorResponse x)
        {
            failedResponse = new FailedResponse(x.Stack[0].Detail, x.SystemError);
        }

        public ExecutionResult(RTSStatusResponse x, bool isSuccessful)
        {
            if(isSuccessful)
                successResponse = new SuccessResponse(x);
            else
                failedResponse = new FailedResponse(x);
        }
        public ExecutionResult(bool isSuccessful)
        {
            if (!isSuccessful)
                failedResponse = new FailedResponse {  ErrorMessage = "Transaction not found", ErrorDetail= "Transaction not found: Please contact vendtech or submit a ticket" };
        }
        public ExecutionResult()
        {
                
        }

        public ExecutionResult InitializeRequestAndResponse(string requestAsString, string responseAsString)
        {
            request = requestAsString;
            response = responseAsString;
            return this;
        }
    }

}
