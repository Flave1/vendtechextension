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

        public async Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, Guid integratorid, string integratorName)
        {
            try
            {
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);

                await _repository.SalesInternalValidation(wallet, request, integratorid);

                Transaction transactionLog = await _repository.CreateSaleTransactionLog(request, integratorid);

                await _repository.DeductFromWallet(wallet, transactionLog);

                ElectricitySaleRTO requestDto = TransferRequestToRTO(request, transactionLog.VendtechTransactionID);

                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(requestDto, integratorid, integratorName);

                if (executionResult.Status == "success")
                {
                    executionResult.SuccessResponse.UpdateResponse(transactionLog);
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transactionLog);
                    return Response.WithStatus(executionResult.Status).WithStatusCode(executionResult.StatusCode).WithMessage(executionResult.SuccessResponse.Voucher?.VendStatusDescription).WithType(executionResult).GenerateResponse();
                }
                else if (executionResult.Status == "pending")
                {
                    AddSaleToQueue(transactionLog.TransactionUniqueId, transactionLog.VendtechTransactionID, integratorid, integratorName);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                    return Response.WithStatus(executionResult.Status).WithStatusCode(executionResult.StatusCode).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
                }
                else
                {
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                    await _repository.RefundToWallet(wallet, transactionLog);
                    return Response.WithStatus(executionResult.Status).WithStatusCode(executionResult.StatusCode).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
                }
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = new ExecutionResult();
                executionResult.FailedResponse = new FailedResponse();
                executionResult.FailedResponse.ErrorMessage = ex.Message;
                executionResult.FailedResponse.ErrorDetail = ex.Message;
                return Response.WithStatus("failed").WithStatusCode(200).WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
            }
        }
        public async Task<APIResponse> QuerySalesStatus(SaleStatusRequest request, Guid integratorid, string integratorName)
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
                        executionResult.Status = "failed";
                    }
                }
                else if (transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.Status = "success";
                }
                else if (!transaction.Finalized)
                {
                    executionResult = await _executionContext.ExecuteTransaction(transaction.VendtechTransactionID, integratorid, integratorName);
                }
                return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage("").WithType(executionResult).GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = new ExecutionResult();
                executionResult.FailedResponse = new FailedResponse();
                executionResult.FailedResponse.ErrorMessage = ex.Message;
                executionResult.FailedResponse.ErrorDetail = ex.Message;
                return Response.WithStatus("failed").WithStatusCode(200).WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
            }
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
                    executionResult.Status = "failed";
                    executionResult.StatusCode = API_MESSAGE_CONSTANCE.BAD_REQUEST;

                    await _repository.RefundToWallet(wallet, transactionLog);
                }
                else if (existingTransaction.Finalized)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.Status = "success";
                    executionResult.SuccessResponse.UpdateResponse(transactionLog);
                    executionResult.StatusCode = API_MESSAGE_CONSTANCE.OKAY_REQEUST;
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transactionLog);
                }
                else if (!existingTransaction.Finalized)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.Status = "pending";
                    executionResult.StatusCode = API_MESSAGE_CONSTANCE.OKAY_REQEUST;
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                }
                return Response.WithStatus(executionResult.Status).WithStatusCode(executionResult.StatusCode).WithMessage("").WithType(executionResult).GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = new ExecutionResult();
                executionResult.FailedResponse = new FailedResponse();
                executionResult.FailedResponse.ErrorMessage = ex.Message;
                executionResult.FailedResponse.ErrorDetail = ex.Message;
                return Response.WithStatus("failed").WithStatusCode(200).WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
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
                        executionResult.Status = "failed";
                    }
                }
                else if (transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.Status = "success";
                }
                else if (!transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.Status = "pending";
                    //executionResult = await _executionContext.ExecuteTransaction(transaction.VendtechTransactionID, integratorid, integratorName);
                }
                return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage("").WithType(executionResult).GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = new ExecutionResult();
                executionResult.FailedResponse = new FailedResponse();
                executionResult.FailedResponse.ErrorMessage = ex.Message;
                executionResult.FailedResponse.ErrorDetail = ex.Message;
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

                if (executionResult.Status == "success")
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
