using Hangfire;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class ElectricitySalesService: BaseService, IElectricitySalesService
    {
        private readonly RequestExecutionContext _executionContext; 
        private readonly TransactionRepository _transactionRepository;
        private readonly IRecurringJobManager _recurringJobManager;
        public ElectricitySalesService(RequestExecutionContext executionContext, TransactionRepository transactionRepository, IRecurringJobManager recurringJobManager)
        {
            _executionContext = executionContext;
            _transactionRepository = transactionRepository;
            _recurringJobManager = recurringJobManager;
        }

        public async Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, string integratorid, string integratorName)
        {
            await _transactionRepository.InternalValidation(request, integratorid);

            Transaction transactionLog = await _transactionRepository.CreateTransactionLog(request, integratorid);

            ExecutionResult executionResult = await _executionContext.ExecuteTransaction(request, integratorid, integratorName);
            if (executionResult.Status == "success")
            {
                executionResult.SuccessResponse.UpdateResponse(transactionLog);
                await _transactionRepository.UpdateSuccessTransactionLog(executionResult, transactionLog);
                return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.SuccessResponse.Voucher?.VendStatusDescription).WithType(executionResult).GenerateResponse();
            }
            else if (executionResult.Status == "pending")
            {
                AddSaleToQueue(request.TransactionId, integratorid, integratorName);
                await _transactionRepository.UpdateFailedTransactionLog(executionResult, transactionLog);
                return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
            }
            else
            {
                await _transactionRepository.UpdateFailedTransactionLog(executionResult, transactionLog);
                return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
            }
        }

        public async Task<APIResponse> QuerySalesStatus(SaleStatusRequest request, string integratorid, string integratorName)
        {
            ExecutionResult executionResult = null;
            Transaction transaction = await _transactionRepository.GetTransaction(request.TransactionId, integratorid);
            
            if(transaction == null)
                executionResult = await _executionContext.ExecuteTransaction(request.TransactionId, integratorid, integratorName);
            else if (transaction.Finalized)
            {
                executionResult = new ExecutionResult(transaction.Response, transaction.ReceivedFrom);
                executionResult.Status = "success";
            }
            else if (!transaction.Finalized)
            {
                executionResult = await _executionContext.ExecuteTransaction(request.TransactionId, integratorid, integratorName);
            }
            return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage("").WithType(executionResult).GenerateResponse();
        }

        private void AddSaleToQueue(string transactionId, string integratorId, string integratorName)
        {
            string jobId = $"{integratorName}_{transactionId}";
            _recurringJobManager.AddOrUpdate(jobId, () => CheckPendingSalesStatusJob(transactionId, integratorId, integratorName), Cron.Minutely);
            _recurringJobManager.Trigger(jobId);
        }
        public Task CheckPendingSalesStatusJob(string transactionId, string integratorId, string integratorName)
        {
            return CheckPendingSalesStatus(transactionId, integratorId, integratorName);
        }
        public async Task CheckPendingSalesStatus(string transactionId, string integratorId, string integratorName)
        {
            Transaction transaction = await _transactionRepository.GetTransaction(transactionId, integratorId);

            if(transaction != null) 
            {
                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(transactionId, integratorId, integratorName);

                if (executionResult.Status == "success")
                {
                    _recurringJobManager.RemoveIfExists($"{integratorName}_{transactionId}");
                    await _transactionRepository.UpdateSuccessTransactionLog(executionResult, transaction);
                }
                else
                {
                    await _transactionRepository.UpdateFailedTransactionLog(executionResult, transaction);
                }
            }

            await Task.CompletedTask;
        }

    }
}
