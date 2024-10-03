﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
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
        private readonly ILogService _log;

        public string requestAsString;
        public string responseAsString;
        public ExecutionResult salesResponse;

        //........................................................
        //WILL CHANGE INTEGRATOR (EASY TO DO)
        private RTSProperties _integrator;
        private RTSInformation _rts;
        //WILL CHANGE INTEGRATOR (EASY TO DO)
        //........................................................

        public RequestExecutionContext(HttpRequestService webRequest, IOptions<RTSInformation> rts, ILogService log)
        {
            _webRequest = webRequest;
            _integrator = RTSProperties.Instance;
            _rts = rts.Value;
            _log = log;
            _integratorInfor = new IntegratorInProcessInformation();
        }
        private void InitializeUrl()
        {
            _url = _integrator.GetProductionUrl();
        }
        public void InitializeIntegratorData(string id, string name, string transactionId, decimal? amount, string meterNumber)
        {
            _integrator.rts = _rts;
            _requestObject = _integrator.GenerateSaleRequest(amount, meterNumber, transactionId);
            requestAsString = JsonConvert.SerializeObject(_requestObject);

            _integratorInfor.CopyData(id, name);
            InitializeUrl();
        }
        public void InitializeIntegratorData(string id, string name, string transactionId)
        {
            _integrator.rts = _rts;
            _requestObject = _integrator.GenerateSaleStatusRequest(transactionId);
            requestAsString = JsonConvert.SerializeObject(_requestObject);

            _integratorInfor.CopyData(id, name);
            InitializeUrl();
        }
        public async Task ExecuteRequest()
        {
            _httpResponse = await _webRequest.SendPostAsync(_url, _requestObject);
        }
        public async Task<ExecutionResult> ExecuteTransaction(ElectricitySaleRequest request, string integratorId, string integratorName)
        {
            InitializeIntegratorData(integratorId, integratorName, request.TransactionId, request.Amount, request.MeterNumber);

            _log.Log(LogType.Infor, $"executing request for {request.TransactionId} from {integratorName}", requestAsString);
            await ExecuteRequest();

            await ProcessResponse();
            _log.Log(LogType.Infor, $"executed request for {request.TransactionId} from {integratorName}", responseAsString);

            ExecutionResult executionResult = salesResponse;
            executionResult.InitializeRequestAndResponse(requestAsString, responseAsString);

            return executionResult;
        }
        public async Task<ExecutionResult> ExecuteTransaction(string transactionId, string integratorId, string integratorName)
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
                string errorMessage = _integrator.ReadErrorMessage(salesResponse.FailedResponse.ErrorMessage);
                
                if (errorMessage == "pending")
                    salesResponse.Status = errorMessage;
            }
            _integrator.Dispose();
            salesResponse.ReceivedFrom = _integrator.ReceivedFrom;
            return salesResponse;
        }
        public async Task<ExecutionResult> ProcessStatusResponse()
        {
            string resultAsString = await _httpResponse.Content.ReadAsStringAsync();
            responseAsString = resultAsString;
            _integrator.ProcessStatusResponse(resultAsString);
            if (_integrator.isSuccessful)
            {
                salesResponse = new ExecutionResult(_integrator.statusResponse, _integrator.isSuccessful);
                salesResponse.Status = "success";
            }
            else
            {
                salesResponse = new ExecutionResult(_integrator.statusResponse, _integrator.isSuccessful);
                salesResponse.Status = "failed";
                _integrator.ReadErrorMessage(salesResponse.FailedResponse.ErrorMessage);
            }
            _integrator.Dispose();
            salesResponse.ReceivedFrom = _integrator.ReceivedFrom;
            return salesResponse;
        }
    }
}
