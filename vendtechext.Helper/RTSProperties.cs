using Newtonsoft.Json;
using vendtechext.Contracts;
using vendtechext.Contracts.VtchMainModels;
using vendtechext.DAL.Common;
using vendtechext.Helper.Configurations;

namespace vendtechext.Helper
{
    public class RTSProperties : IDisposable
    {
        // Static Lazy Singleton
        private static readonly Lazy<RTSProperties> _instance = new Lazy<RTSProperties>(() => new RTSProperties());

        public string ReceivedFrom;
        private decimal _amount;
        private string _meterNumber;
        private string _transactionId;

        public RTSResponse successResponse = null;
        public RTSErorResponse errorResponse = null;
        public RTSStatusResponse statusResponse = null;
        public ProviderInformation rts;
        public bool isSuccessful = false;
        public bool isFinalized = false;

        // Private constructor to prevent direct instantiation
        private RTSProperties()
        {

        }

        // Public accessor for the Singleton instance
        public static RTSProperties Instance => _instance.Value;

        public string GetProductionUrl() => rts.ProductionUrl;

        public string GetSandboxUrl() => rts.SandboxUrl;

        public string GetTransactionId() => _transactionId;

        public object GenerateSaleRequest(decimal? amount, string meterNumber, string transactionId)
        {
            _amount = amount.Value;
            _meterNumber = meterNumber;
            _transactionId = transactionId;
            return new
            {
                Auth = new
                {
                    rts.UserName,
                    rts.Password,
                },
                Request = "ProcessPrePaidVendingV1",
                Parameters = new object[]
                                   {
                        new
                        {
                            rts.UserName,
                            rts.Password,
                            System = "SL"
                        }, "apiV1_VendVoucher", "webapp", "0", "EDSA", _amount, _meterNumber, -1, "ver1.5", _transactionId
                     },
            };
        }

        public object GenerateSaleStatusRequest(string transactionId)
        {
            _transactionId = transactionId;
            return new
            {
                Auth = new 
                {
                    Password = rts.Password,
                    UserName = rts.UserName
                },
                Request = "ProcessPrePaidVendingV1",
                Parameters = new object[]
                                     {
                        new
                        {
                             Password = rts.Password,
                             UserName = rts.UserName,
                             System = "SL"
                        }, "apiV1_GetTransactionStatus", _transactionId
                       },
            };
        }
        
        public void ProcessResponse(string resultAsString)
        {
            ReceivedFrom = "rts_init";
            try
            {
                isSuccessful = true;
                successResponse = JsonConvert.DeserializeObject<RTSResponse>(resultAsString);
            }
            catch (JsonSerializationException)
            {
                isSuccessful = false;
                errorResponse = JsonConvert.DeserializeObject<RTSErorResponse>(resultAsString);
            }
        }

        public void ProcessStatusResponse(string resultAsString)
        {
            ReceivedFrom = "rts_status";
            statusResponse = JsonConvert.DeserializeObject<RTSStatusResponse>(resultAsString);
            if (string.IsNullOrEmpty(statusResponse.Content.VoucherPin))
            {
                isSuccessful = false;
                isFinalized = statusResponse.Content.Finalised;
            }
            else
                isSuccessful = true;
        }
        public int ReadErrorMessage(string message)
        {
            if (message == "The request timed out with the Ouc server.")
            {
                return API_MESSAGE_CONSTANCE.REQUEST_TIMEOUT;
            }
            if (message == "Error: Vending is disabled")
            {
                return API_MESSAGE_CONSTANCE.VENDING_DISABLE;
            }

            if (message == "-9137 : InCMS-BL-CB001607. Purchase not allowed, not enought vendor balance")
            {
                return API_MESSAGE_CONSTANCE.AMOUNT_TOO_LOW;
            }

            if (message == "InCMS-BL-CO000846. The amount is too low for recharge")
            {
                return API_MESSAGE_CONSTANCE.AMOUNT_TOO_LOW;
            }

            if (message == "-47 : InCMS-BL-CB001273. Error, purchase units less than minimum.")
            {
                return API_MESSAGE_CONSTANCE.AMOUNT_TOO_LOW;
            }
            return API_MESSAGE_CONSTANCE.BAD_REQUEST;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
