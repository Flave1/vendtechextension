﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;
using vendtechext.Helper.Configurations;

namespace vendtechext.Helper
{
    public class RequestExecutionContext
    {
        private object _requestObject;
        private string _url;
        private HttpResponseMessage _httpResponse;
        private readonly HttpRequestService _webRequest;
        private readonly IntegratorInProcessInformation _integratorInfor;
        private readonly LogService _log;
        private readonly ProviderInformation _providerInfor;

        //........................................................
        //WILL CHANGE INTEGRATOR (EASY TO DO)
        private readonly RTSProperties _integrator;
        //WILL CHANGE INTEGRATOR (EASY TO DO)
        //........................................................

        public string requestAsString;
        public string responseAsString;
        public ExecutionResult salesResponse;


        public RequestExecutionContext(HttpRequestService webRequest, IOptions<ProviderInformation> rts, LogService log)
        {
            _webRequest = webRequest;
            _integrator = RTSProperties.Instance;
            _providerInfor = rts.Value;
            _log = log;
            _integratorInfor = new IntegratorInProcessInformation();
        }
        private void InitializeUrl()
        {
            _url = _integrator.GetProductionUrl();
        }
        public void InitializeIntegratorData(Guid id, string name, string transactionId, decimal? amount, string meterNumber)
        {
            _integrator.rts = _providerInfor;
            _requestObject = _integrator.GenerateSaleRequest(amount, meterNumber, transactionId);
            requestAsString = JsonConvert.SerializeObject(_requestObject);

            _integratorInfor.CopyData(id, name);
            InitializeUrl();
        }
        public void InitializeIntegratorData(Guid id, string name, string transactionId)
        {
            _integrator.rts = _providerInfor;
            _requestObject = _integrator.GenerateSaleStatusRequest(transactionId);
            requestAsString = JsonConvert.SerializeObject(_requestObject);

            _integratorInfor.CopyData(id, name);
            InitializeUrl();
        }
        public async Task ExecuteRequest()
        {
            _httpResponse = await _webRequest.SendPostAsync(_url, _requestObject);
        }
        public async Task<ExecutionResult> ExecuteTransaction(ElectricitySaleRTO request, Guid integratorId, string integratorName)
        {
            InitializeIntegratorData(integratorId, integratorName, request.VendtechTransactionId, request.Amount, request.MeterNumber);

            _log.Log(LogType.Infor, $"executing request for {request.TransactionId} from {integratorName}", requestAsString);
            await ExecuteRequest();

            await ProcessResponse();
            _log.Log(LogType.Infor, $"executed request for {request.TransactionId} from {integratorName}", responseAsString);

            ExecutionResult executionResult = salesResponse;
            executionResult.InitializeRequestAndResponse(requestAsString, responseAsString);

            return executionResult;
        }
        public async Task<ExecutionResult> ExecuteTransaction(string transactionId, Guid integratorId, string integratorName)
        {
            InitializeIntegratorData(integratorId, integratorName, transactionId);

            await ExecuteRequest();

            await ProcessStatusResponse();

            ExecutionResult executionResult = salesResponse;
            executionResult.InitializeRequestAndResponse(requestAsString, responseAsString);

            return executionResult;
        }
        public async Task<ExecutionResult> ProcessResponse()
        {
            string resultAsString = await _httpResponse.Content.ReadAsStringAsync();
            responseAsString = resultAsString;
            _integrator.DestructureInitialResponse(resultAsString);
            if (_integrator.isSuccessful)
            {
                salesResponse = new ExecutionResult(_integrator.successResponse);
                salesResponse.status = "success";
                salesResponse.code = API_MESSAGE_CONSTANCE.OKAY_REQEUST;
            }
            else
            {
                salesResponse = new ExecutionResult(_integrator.errorResponse);
                salesResponse.status = "failed";
                
                if (_integrator.isFinalized)
                    salesResponse.status = "pending";

                salesResponse.code = _integrator.ReadErrorAndReturnStatusCode(salesResponse.failedResponse.ErrorMessage);
            }
            _integrator.Dispose();
            salesResponse.receivedFrom = _integrator.ReceivedFrom;
            return salesResponse;
        }
   
        public async Task<ExecutionResult> ProcessStatusResponse()
        {
            string resultAsString = await _httpResponse.Content.ReadAsStringAsync();
            responseAsString = resultAsString;
            _integrator.DestructureStatusResponse(resultAsString);
            if (_integrator.isSuccessful)
            {
                salesResponse = new ExecutionResult(_integrator.statusResponse, _integrator.isSuccessful);
                salesResponse.status = "success";
            }
            else
            {
                if (!_integrator.isFinalized)
                {

                    salesResponse = new ExecutionResult(_integrator.statusResponse, _integrator.isSuccessful);
                    salesResponse.status = "pending";
                }
                else
                {
                    salesResponse = new ExecutionResult(_integrator.statusResponse, _integrator.isSuccessful);
                    salesResponse.status = "failed";
                }
            }
            _integrator.Dispose();
            salesResponse.receivedFrom = _integrator.ReceivedFrom;
            return salesResponse;
        }
    }
}
