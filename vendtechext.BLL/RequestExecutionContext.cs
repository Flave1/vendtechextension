using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using signalrserver.Models.DTO;
using vendtechext.BLL.Common;
using vendtechext.BLL.Configurations;
using vendtechext.BLL.Interfaces;

namespace vendtechext.BLL
{
    public class RequestExecutionContext
    {
        private object _requestObject;
        private string _url;
        private HttpResponseMessage _httpResponse;
        private readonly IHttpRequestService _webRequest;
        public ExecutionResult salesResponse;

        public string requestAsString;
        public string responseAsString;

        //Will Change When Connecting To A Differrent Integrator
        private RTSProperties _integrator;
        private RTSInformation _rts;
        public RequestExecutionContext(IHttpRequestService webRequest, IOptions<RTSInformation> rts)
        {
            _webRequest = webRequest;
            _integrator = RTSProperties.Instance;
            _rts = rts.Value;
        }

        public void BuildRequest(decimal amount, string meterNumber, string transactionId)
        {
            _integrator.rts = _rts;
            _requestObject = _integrator.GenerateSaleRequest(amount, meterNumber, transactionId);
            requestAsString = JsonConvert.SerializeObject(_requestObject);
            InitializeUrl();
        }
        public async Task ExecuteRequest()
        {
            _httpResponse = await _webRequest.SendPostAsync(_url, _requestObject);
        }

        public async Task<ExecutionResult> ProcessResponse()
        {
            string resultAsString = await _httpResponse.Content.ReadAsStringAsync();
            responseAsString = resultAsString;
            _integrator.ProcessResponse(resultAsString);
            if (_integrator.isSuccessful)
            {
                salesResponse = new ExecutionResult(_integrator.successResponse);
                salesResponse.Status = "success";
            }
            else
            {
                salesResponse = new ExecutionResult(_integrator.errorResponse);
                salesResponse.Status = "failed";
            }
            _integrator.Dispose();
            return salesResponse;
        }

        private void InitializeUrl()
        {
            _url = _integrator.GetProductionUrl();
        }
    }
}
