using Hangfire;
using Microsoft.EntityFrameworkCore.Storage;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
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
        public APISalesService(RequestExecutionContext executionContext, TransactionRepository transactionRepository, IRecurringJobManager recurringJobManager, WalletRepository walletReo)
        {
            _executionContext = executionContext;
            _repository = transactionRepository;
            _recurringJobManager = recurringJobManager;
            _walletReo = walletReo;
        }

        public async Task<APIResponse> PurchaseElectricity(ElectricitySaleRequest request, Guid integratorid, string integratorName)
        {
            try
            {
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);

                await _repository.SalesInternalValidation(wallet, request, integratorid);

                Transaction transactionLog = await _repository.CreateSaleTransactionLog(request, integratorid);

                await _repository.DeductFromWallet(wallet, transactionLog);
                request.TransactionId = transactionLog.VendtechTransactionID;

                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(request, integratorid, integratorName);
                if (executionResult.Status == "success")
                {
                    executionResult.SuccessResponse.UpdateResponse(transactionLog);
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transactionLog);
                    return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.SuccessResponse.Voucher?.VendStatusDescription).WithType(executionResult).GenerateResponse();
                }
                else if (executionResult.Status == "pending")
                {
                    AddSaleToQueue(transactionLog.TransactionUniqueId, integratorid, integratorName);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                    return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
                }
                else
                {
                    await _repository.RefundToWallet(wallet, transactionLog);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                    return Response.WithStatus(executionResult.Status).WithStatusCode(200).WithMessage(executionResult.FailedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
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

                if (existingTransaction == null)
                {
                    executionResult = new ExecutionResult(false);
                    executionResult.Status = "failed";
                }
                else if (existingTransaction.Finalized)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.Status = "success";
                }
                else if (!existingTransaction.Finalized)
                {
                    executionResult = await _executionContext.ExecuteTransaction(existingTransaction.VendtechTransactionID, integratorid, integratorName);
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

        private void AddSaleToQueue(string transactionId, Guid integratorId, string integratorName)
        {
            string jobId = $"{integratorName}_{transactionId}";
            _recurringJobManager.AddOrUpdate(jobId, () => CheckPendingSalesStatusJob(transactionId, integratorId, integratorName), Cron.Minutely);
            _recurringJobManager.Trigger(jobId);
        }
        public Task CheckPendingSalesStatusJob(string transactionId, Guid integratorId, string integratorName)
        {
            return CheckPendingSalesStatus(transactionId, integratorId, integratorName);
        }
        public async Task CheckPendingSalesStatus(string transactionId, Guid integratorId, string integratorName)
        {
            Transaction transaction = await _repository.GetSaleTransaction(transactionId, integratorId);

            if(transaction != null) 
            {
                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(transactionId, integratorId, integratorName);

                if (executionResult.Status == "success")
                {
                    _recurringJobManager.RemoveIfExists($"{integratorName}_{transactionId}");
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transaction);
                }
                else
                {
                    Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorId);
                    await _repository.RefundToWallet(wallet, transaction);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transaction);
                }
            }

            await Task.CompletedTask;
        }

    }
}
