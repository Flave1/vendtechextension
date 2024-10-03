using Newtonsoft.Json;
using vendtechext.Contracts;
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
        public RTSInformation rts;
        public bool isSuccessful = false;

        // Private constructor to prevent direct instantiation
        private RTSProperties()
        {

        }

        // Public accessor for the Singleton instance
        public static RTSProperties Instance => _instance.Value;

        public string GetProductionUrl() => rts.ProductionUrl;

        public string GetSandboxUrl() => rts.SandboxBox;

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
                isSuccessful = false;
            else
                isSuccessful = true;
        }

        public string ReadErrorMessage(string message)
        {
            if (message == "The request timed out with the Ouc server.")
            {
                return message;
            }
            else if (message == "Error: Vending is disabled")
            {
                return message;
            }
            else if (message == "-9137 : InCMS-BL-CB001607. Purchase not allowed, not enought vendor balance")
            {
                return "Due to some technical resolutions involving EDSA, the system is unable to vend";
            }

            else if (message == "InCMS-BL-CO000846. The amount is too low for recharge")
            {
                return "The amount is too low for recharge";
            }
            else if (message == "Unexpected error in OUC VendVoucher")
            {
                return message;
            }
            else if (message == "CB001600 : InCMS-BL-CB001600. Error serial number, contracted service not found or not active.")
            {
                return "Error serial number, contracted service not found or not active";
            }
            else if (message == "There was an error when determining if the request for the given meter number can be processed.")
            {
                return message;
            }
            else if (message == "Input string was not in a correct format.")
            {
                return "Amount tendered is too low";
            }
            else if (message == "-47 : InCMS-BL-CB001273. Error, purchase units less than minimum.")
            {
                return "Purchase units less than minimum.";
            }
            else if (message == "The specified TransactionID already exists for this terminal.")
                return message;
            else
            {
                return "pending";
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
