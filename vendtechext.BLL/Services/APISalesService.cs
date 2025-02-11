using Hangfire;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
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

                await _repository.DeductFromWallet(transactionId: transactionLog.Id, walletId: wallet.Id);

                ElectricitySaleRTO requestDto = TransferRequestToRTO(request, transactionLog.VendtechTransactionID);

                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(requestDto, integratorid, integratorName);

                if (executionResult.status == "success")
                {
                    executionResult.successResponse.UpdateResponse(transactionLog);
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transactionLog);
                    return Response.WithStatus(executionResult.status).WithMessage("Vend was successful").WithType(executionResult).GenerateResponse();
                }
                else if (executionResult.status == "pending")
                {
                    AddSaleToQueue(transactionLog.TransactionUniqueId, transactionLog.VendtechTransactionID, integratorid, integratorName);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                    return Response.WithStatus(executionResult.status).WithMessage(executionResult.failedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
                }
                else
                {
                    if(executionResult.code == API_MESSAGE_CONSTANCE.VENDING_DISABLE) 
                    {
                        await AppConfiguration.DisableSales();
                    }
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                    await _repository.RefundToWallet(transactionId: transactionLog.Id, walletId: wallet.Id);
                    return Response.WithStatus(executionResult.status).WithMessage(executionResult.failedResponse.ErrorDetail).WithType(executionResult).GenerateResponse();
                }
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(ex, API_MESSAGE_CONSTANCE.BAD_REQUEST);
                return Response.WithStatus("failed").WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
            }
            catch (SystemDisabledException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(ex, API_MESSAGE_CONSTANCE.VENDING_DISABLE);
                return Response.WithStatus("failed").WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
            }
        }
        public async Task<APIResponse> QuerySalesStatus(SaleStatusRequest request, Guid integratorid, string integratorName)
        {
            try
            {
                ExecutionResult executionResult = null;
                Transaction transaction = await _repository.GetSaleTransaction(request.TransactionId, integratorid);
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);

                if (transaction == null)
                {
                    executionResult = GenerateExecutionResult(new BadRequestException("Transaction with specified ID was not found"), API_MESSAGE_CONSTANCE.BAD_REQUEST);
                    executionResult.status = "failed";
                    return Response.WithStatus(executionResult.status).WithMessage(executionResult.failedResponse.ErrorMessage).WithType(executionResult).GenerateResponse();
                }
                else if (transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.status = "success";
                    executionResult.code = API_MESSAGE_CONSTANCE.OKAY_REQEUST;

                    return Response.WithStatus(executionResult.status).WithMessage("Transaction Successfully fetched").WithType(executionResult).GenerateResponse();
                }
                else if (!transaction.Finalized)
                {
                    executionResult = await _executionContext.ExecuteTransaction(transaction.VendtechTransactionID, integratorid, integratorName);
                    if (executionResult.status == "success")
                    {
                        executionResult.successResponse.UpdateResponse(transaction);
                        executionResult.code = API_MESSAGE_CONSTANCE.OKAY_REQEUST;
                        await _repository.UpdateSaleSuccessTransactionLog(executionResult, transaction);
                        await _repository.DeductFromWallet(transactionId: transaction.Id, walletId: wallet.Id);
                        return Response.WithStatus(executionResult.status).WithMessage("Transaction Successfully fetched").WithType(executionResult).GenerateResponse();
                    }
                    else if(executionResult.status == "failed")
                    {
                        await _repository.RefundToWallet(transactionId: transaction.Id, walletId: wallet.Id);
                        executionResult.successResponse.UpdateResponse(transaction);
                        executionResult.code = API_MESSAGE_CONSTANCE.BAD_REQUEST;
                        await _repository.UpdateSaleTransactionLogOnStatusQuery(executionResult, transaction, TransactionStatus.Failed);
                        return Response.WithStatus(executionResult.status).WithMessage("Transaction unsuccessful").WithType(executionResult).GenerateResponse();
                    }
                }
                return Response.WithStatus(executionResult.status).WithMessage(executionResult.failedResponse.ErrorMessage).WithType(executionResult).GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(ex, API_MESSAGE_CONSTANCE.BAD_REQUEST);
                return Response.WithStatus("failed").WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
            }
            catch (SystemDisabledException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(ex, API_MESSAGE_CONSTANCE.VENDING_DISABLE);
                return Response.WithStatus("failed").WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
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
                    executionResult.status = "failed";
                }
                else if (existingTransaction.Finalized)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.status = "success";
                    executionResult.successResponse.UpdateResponse(transactionLog);
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transactionLog);
                }
                else if (!existingTransaction.Finalized)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.status = "pending";
                    AddSaleToQueue(transactionLog.TransactionUniqueId, transactionLog.VendtechTransactionID, integratorid, integratorName);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transactionLog);
                }
                return Response.WithStatus(executionResult.status).WithMessage("").WithType(executionResult).GenerateResponse();
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
                return Response.WithStatus(executionResult.status).WithMessage("").WithType(executionResult).GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = new ExecutionResult();
                executionResult.failedResponse = new FailedResponse();
                executionResult.failedResponse.ErrorMessage = ex.Message;
                executionResult.failedResponse.ErrorDetail = ex.Message;
                return Response.WithStatus("failed").WithMessage(ex.Message).WithType(executionResult).GenerateResponse();
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
                    Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorId);
                    await _repository.DeductFromWalletIfRefunded(transactionId: transaction.Id, walletId: wallet.Id);
                    transaction.ClaimedStatus = (int)ClaimedStatus.Unclaimed;
                    await _repository.UpdateSaleSuccessTransactionLog(executionResult, transaction);
                }
                else if (executionResult.status == "pending")
                {
                    _logService.Log(LogType.QeueJob, $"Re-Running job {jobId}", transaction);
                }
                else
                {
                    _logService.Log(LogType.QeueJob, $"Re-Running job {jobId}", transaction);
                    Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorId);
                    await _repository.RefundToWallet(transactionId: transaction.Id, walletId: wallet.Id);
                    await _repository.UpdateSaleFailedTransactionLog(executionResult, transaction);
                    _recurringJobManager.RemoveIfExists(jobId);
                }
            }

            await Task.CompletedTask;
        }

        private ElectricitySaleRTO TransferRequestToRTO(ElectricitySaleRequest x, string vendtechTransactionId) => 
            new ElectricitySaleRTO { Amount = x.Amount, MeterNumber = x.MeterNumber, TransactionId = x.TransactionId, VendtechTransactionId = vendtechTransactionId };
    }
}
