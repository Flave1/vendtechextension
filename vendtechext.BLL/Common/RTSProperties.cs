using Azure;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using vendtechext.BLL.Configurations;
using vendtechext.BLL.DTO;

namespace vendtechext.BLL.Common
{
    public class RTSProperties : IDisposable
    {
        // Static Lazy Singleton
        private static readonly Lazy<RTSProperties> _instance = new Lazy<RTSProperties>(() => new RTSProperties());

        private decimal _amount;
        private string _meterNumber;
        private string _transactionId;

        public RTSResponse successResponse = null;
        public RTSErorResponse errorResponse = null;
        public RTSInformation rts;
        public bool isSuccessful = false;

        // Private constructor to prevent direct instantiation
        private RTSProperties()
        {
                
        }

        // Public accessor for the Singleton instance
        public static RTSProperties Instance => _instance.Value;

        public string GetProductionUrl() =>  rts.ProductionUrl;

        public string GetSandboxUrl() =>  rts.SandboxBox;

        public object GenerateSaleRequest(decimal amount, string meterNumber, string transactionId)
        {
            _amount = amount;
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

        public void ProcessResponse(string resultAsString)
        {
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
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
