using Microsoft.EntityFrameworkCore;
using signalrserver.Models.DTO;
using vendtechext.BLL.Common;
using vendtechext.BLL.DTO;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class ElectricitySalesService: IElectricitySalesService
    {
        private readonly DataContext _dataContext;
        private readonly RequestExecutionContext executionContext;
        private ILogService _log;
        public ElectricitySalesService(DataContext dtcxt, RequestExecutionContext executionContext, ILogService log)
        {
            _dataContext = dtcxt;
            this.executionContext = executionContext;
            _log = log;
        }

        public async Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, string integratorid)
        {

            var vtechSalesService = new VendtechTransactionsService();
            var tranx = await vtechSalesService.CreateRecordBeforeVend(request.MeterNumber, request.Amount);

            await InternalValidation(request, integratorid);

            request.TransactionId = tranx.TransactionId;
            Transaction transactionLog = await CreateTransactionLog(request, integratorid);

            var executionResult = await ExecuteTransaction(request);
            if (executionResult.Status == "success")
            {
                executionResult.SuccessResponse.UpdateResponse(transactionLog);
                await UpdateSuccessTransactionLog(executionResult, transactionLog);
                return Response.Instance.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.SuccessResponse.VendStatus).WithType(executionResult).GenerateResponse();
            }
            else
            {
                await UpdateFailedTransactionLog(executionResult, transactionLog);
                return Response.Instance.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
            }
        }

        private async Task<Transaction> CreateTransactionLog(ElectricitySaleRequest request, string integratorId)
        {
            var trans = new TransactionsBuilder()
                .WithTransactionId(UniqueIDGenerator.NewTransactionId())
                .WithTransactionStatus(TransactionStatus.Pending)
                .WithTerminalId(request.TransactionId)
                .WithMeterNumber(request.MeterNumber)
                .WithIntegratorId(integratorId)
                .WithCreatedAt(DateTime.Now)
                .WithAmount(request.Amount)
                .Build();

            _dataContext.Transactions.Add(trans);
            await _dataContext.SaveChangesAsync();
            return trans;
        }

        private async Task UpdateSuccessTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithCustomerAddress(executionResult.SuccessResponse.CustomerAddress)
                .WithAccountNumber(executionResult.SuccessResponse.AccountNumber)
                .WithReceiptNumber(executionResult.SuccessResponse.ReceiptNumber)
                .WithServiceCharge(executionResult.SuccessResponse.ServiceCharge)
                .WithSerialNumber(executionResult.SuccessResponse.SerialNumber)
                .WithMeterToken1(executionResult.SuccessResponse.MeterToken1)
                .WithMeterToken2(executionResult.SuccessResponse.MeterToken1)
                .WithMeterToken3(executionResult.SuccessResponse.MeterToken1)
                .WithCostOfUnits(executionResult.SuccessResponse.CostOfUnits)
                .WithRTSUniqueID(executionResult.SuccessResponse.RTSUniqueID)
                .WithTaxCharge(executionResult.SuccessResponse.TaxCharge)
                .WithVProvider(executionResult.SuccessResponse.VProvider)
                .WithTransactionStatus(TransactionStatus.Success)
                .WithTariff(executionResult.SuccessResponse.Tariff)
                .WithUnits(executionResult.SuccessResponse.Units)
                .WithResponse(executionResult.Response)
                .WithRequest(executionResult.Request) 
                .WithFinalised(true)
                .WithSold(true)
                .Build();

            await _dataContext.SaveChangesAsync();
        }

        private async Task UpdateFailedTransactionLog(ExecutionResult executionResult, Transaction trans)
        {
            new TransactionsBuilder(trans)
                .WithVendStatusDescription(executionResult.FailedResponse.ErrorDetail)
                .WithStatusResponse(executionResult.FailedResponse.ErrorMessage)
                .WithTransactionStatus(TransactionStatus.Failed)
                .WithResponse(executionResult.Response)
                .WithRequest(executionResult.Request)
                .Build();

            await _dataContext.SaveChangesAsync();
        }
        private async Task<ExecutionResult> ExecuteTransaction(ElectricitySaleRequest request)
        {
            executionContext.BuildRequest(request.Amount, request.MeterNumber, request.TransactionId);

            _log.Log(LogType.Infor, $"executing request for {request.TransactionId}", executionContext.requestAsString);
            await executionContext.ExecuteRequest();

            await executionContext.ProcessResponse();
            _log.Log(LogType.Infor, $"execution response for {request.TransactionId}", executionContext.responseAsString);

            ExecutionResult executionResult = executionContext.salesResponse;
            executionResult.InitializeRequestAndResponse(executionContext);

            return executionResult;
        }

        private async Task InternalValidation(ElectricitySaleRequest request, string integrator)
        {
            //Check for balance

            //check if transactionid exist for this terminal
            if (await _dataContext.Transactions.AnyAsync(d => d.IntegratorId == integrator && d.TransactionId == request.TransactionId))
                throw new BadRequestException("Transaction ID already exist for this terminal.");
        }
    }
}
