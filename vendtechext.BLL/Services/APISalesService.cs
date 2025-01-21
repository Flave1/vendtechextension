using Hangfire;
using Microsoft.EntityFrameworkCore.Storage;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.Contracts.VtchMainModels;
using vendtechext.DAL.Common;
using vendtechext.DAL.Migrations;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class APISalesService: BaseService, IAPISalesService
    {
        private readonly RequestExecutionContext _executionContext; 
        private readonly TransactionRepository _repository;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly WalletRepository _walletReo;
        private readonly LogService _logService;
        public APISalesService(RequestExecutionContext executionContext, TransactionRepository transactionRepository, IRecurringJobManager recurringJobManager, WalletRepository walletReo, LogService logService)
        {
            _executionContext = executionContext;
            _repository = transactionRepository;
            _recurringJobManager = recurringJobManager;
            _walletReo = walletReo;
            _logService = logService;
        }

        public async Task<APIResponse> PurchaseElectricityForSandbox(ElectricitySaleRequest request, Guid integratorid, string integratorName)
        {
            try
            {
                ExecutionResult executionResult = null;
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                await _repository.SalesInternalValidation(wallet, request, integratorid);
                Transaction existingTransaction = await _repository.GetSaleTransactionByRandom(request.MeterNumber);
                Transaction transactionLog = await _repository.CreateSaleTransactionLog(request, integratorid);

                await _repository.DeductFromWallet(wallet, transactionLog);

                if (existingTransaction == null)
                {
                    executionResult = new ExecutionResult(false);
                    executionResult.status = "failed";
                    executionResult.code = API_MESSAGE_CONSTANCE.BAD_REQUEST;

                    await _repository.RefundToWallet(wallet, transactionLog);
                }
                else if (existingTransaction.Finalized)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.status = "success";
                    executionResult.successResponse.UpdateResponse(transactionLog);
                    executionResult.code = API_MESSAGE_CONSTANCE.OKAY_REQEUST;
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transactionLog);
                }
                else if (!existingTransaction.Finalized)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.status = "pending";
                    executionResult.code = API_MESSAGE_CONSTANCE.OKAY_REQEUST;
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                }
                return Response.WithStatus(executionResult.status).WithStatusCode(executionResult.code).WithMessage("").WithType(executionResult).GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = new ExecutionResult();
                executionResult.failedResponse = new FailedResponse();
                executionResult.failedResponse.ErrorMessage = ex.Message;
                executionResult.failedResponse.ErrorDetail = ex.Message;
                return Response.WithStatus("failed").WithStatusCode(400).WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
            }
            }
        public async Task<APIResponse> QuerySalesStatusForSandbox(SaleStatusRequest request, Guid integratorid, string integratorName)
        {
            try
            {
                ExecutionResult executionResult = null;
                Transaction transaction = await _repository.GetSaleTransaction(request.TransactionId, integratorid);

                if (transaction == null)
                {
                    if (request.TransactionId == "131fece5-61be-4bc7-2618-08dceb87f9b5")
                    {
                        executionResult = await _executionContext.ExecuteTransaction(request.TransactionId, integratorid, integratorName);
                    }
                    else
                    {
                        executionResult = new ExecutionResult(false);
                        executionResult.status = "failed";
                        executionResult.code = API_MESSAGE_CONSTANCE.BAD_REQUEST;
                    }
                }
                else if (transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.status = "success";
                    executionResult.code = API_MESSAGE_CONSTANCE.OKAY_REQEUST;
                }
                else if (!transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.status = "pending";
                    executionResult.code = API_MESSAGE_CONSTANCE.REQUEST_PENDING;
                }
                return Response.WithStatus(executionResult.status).WithStatusCode(200).WithMessage("").WithType(executionResult).GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = new ExecutionResult();
                executionResult.failedResponse = new FailedResponse();
                executionResult.failedResponse.ErrorMessage = ex.Message;
                executionResult.failedResponse.ErrorDetail = ex.Message;
                return Response.WithStatus("failed").WithStatusCode(200).WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
            }
        }
        private void AddSaleToQueue(string transactionId, string vtechTransactionId, Guid integratorId, string integratorName)
        {
            string jobId = $"{vtechTransactionId}_{integratorName}_{transactionId}";
            _recurringJobManager.AddOrUpdate(jobId, () => CheckPendingSalesStatusJob(transactionId, vtechTransactionId, integratorId, integratorName), Cron.Minutely);
            _recurringJobManager.Trigger(jobId);
        }
        public Task CheckPendingSalesStatusJob(string transactionId, string vtechTransactionId, Guid integratorId, string integratorName)
        {
            return CheckPendingSalesStatus(transactionId, vtechTransactionId, integratorId, integratorName);
        }
        public async Task CheckPendingSalesStatus(string transactionId, string vtechTransactionId, Guid integratorId, string integratorName)
        {
            string jobId = $"{vtechTransactionId}_{integratorName}_{transactionId}";
            Transaction transaction = await _repository.GetSaleTransaction(transactionId, integratorId);
            _logService.Log(LogType.QeueJob, $"Running job {jobId}", transaction);
            if (transaction != null) 
            {
                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(vtechTransactionId, integratorId, integratorName);

                if (executionResult.status == "success")
                {
                    _logService.Log(LogType.QeueJob, $"Removing job {jobId}", transaction);
                    _recurringJobManager.RemoveIfExists(jobId);
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transaction);
                }
                else
                {
                    _logService.Log(LogType.QeueJob, $"Re-Running job {jobId}", transaction);
                    Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorId);
                    await _repository.RefundToWallet(wallet, transaction);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transaction);
                }
            }

            await Task.CompletedTask;
        }

        private ElectricitySaleRTO TransferRequestToRTO(ElectricitySaleRequest x, string vendtechTransactionId) => 
            new ElectricitySaleRTO { Amount = x.Amount, MeterNumber = x.MeterNumber, TransactionId = x.TransactionId, VendtechTransactionId = vendtechTransactionId };
    }
}
