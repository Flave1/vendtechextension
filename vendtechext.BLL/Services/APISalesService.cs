using Hangfire;
using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.BLL.Services.RecurringJobs;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;
using vendtechext.Helper;
using System.Data;

namespace vendtechext.BLL.Services
{
    public class APISalesService : BaseService, IAPISalesService
    {
        private readonly RequestExecutionContext _executionContext;
        private readonly TransactionRepository _repository;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly WalletRepository _walletReo;
        private readonly LogService _logService;
        private readonly TransactionUpdate _transactionUpdate;

        public APISalesService(
            RequestExecutionContext executionContext,
            TransactionRepository transactionRepository,
            IRecurringJobManager recurringJobManager,
            WalletRepository walletReo,
            LogService logService,
            TransactionUpdate transactionUpdate
        )
        {
            _executionContext = executionContext;
            _repository = transactionRepository;
            _recurringJobManager = recurringJobManager;
            _walletReo = walletReo;
            _logService = logService;
            _transactionUpdate = transactionUpdate;
        }

        #region PRODUCTION


        public async Task<APIResponse> PurchaseElectricity(
            ElectricitySaleRequest request,
            Guid integratorid,
            string integratorName
        )
        {
            try
            {
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                await _repository.SalesInternalValidation(wallet, request, integratorid);

                Transaction transaction = await _repository.CreateSaleTransactionLog(
                    request,
                    integratorid
                );
                transaction = await _repository.DeductFromWallet(
                    transactionId: transaction.Id,
                    walletId: wallet.Id
                );

                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(
                    TransferRequestToRTO(request, transaction.VendtechTransactionID),
                    integratorid,
                    integratorName
                );

                if (executionResult.status == "success")
                {
                    wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                    executionResult.successResponse.UpdateResponse(transaction, wallet);
                    await _transactionUpdate.UpdateSaleSuccessTransactionLog(
                        executionResult,
                        transaction
                    );
                    CheckIntegratorBalanceThreshold(wallet);
                    return Response
                        .WithStatus(executionResult.status)
                        .WithMessage("Vend was successful")
                        .WithType(executionResult)
                        .GenerateResponse();
                }

                if (executionResult.status == "pending")
                {
                    AddSaleToQueue(
                        transaction.TransactionUniqueId,
                        transaction.VendtechTransactionID,
                        integratorid,
                        integratorName
                    );
                    await _transactionUpdate.UpdateSaleFailedTransactionLog(
                        executionResult,
                        transaction
                    );
                    return Response
                        .WithStatus(executionResult.status)
                        .WithMessage(executionResult.failedResponse.ErrorDetail)
                        .WithType(executionResult)
                        .GenerateResponse();
                }

                if (executionResult.code == API_MESSAGE_CONSTANTS.VENDING_DISABLE)
                {
                    await AppConfiguration.DisableSales();
                }
                await _transactionUpdate.UpdateSaleFailedTransactionLog(
                    executionResult,
                    transaction
                );
                transaction = await _repository.RefundToWallet(
                    transactionId: transaction.Id,
                    walletId: wallet.Id
                );

                return Response
                    .WithStatus(executionResult.status)
                    .WithMessage(executionResult.failedResponse.ErrorDetail)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                _logService.Log(LogType.Error, "Bad Request in PurchaseElectricity", ex.ToString());
                ExecutionResult executionResult = GenerateExecutionResult(
                    ex,
                    API_MESSAGE_CONSTANTS.BAD_REQUEST
                );


                return Response
                    .WithStatus("failed")
                    .WithMessage(ex.Message)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
            catch (SystemDisabledException ex)
            {
                _logService.Log(
                    LogType.Error,
                    "System Disabled Exception in PurchaseElectricity",
                    ex.ToString()
                );

                ExecutionResult executionResult = GenerateExecutionResult(
                    ex,
                    API_MESSAGE_CONSTANTS.VENDING_DISABLE
                );

                return Response
                    .WithStatus("failed")
                    .WithMessage(ex.Message)
                    .WithType(executionResult)
                    .GenerateResponse();

            }
        }

        public async Task<APIResponse> QuerySalesStatus(
            SaleStatusRequest request,
            Guid integratorid,
            string integratorName
        )
        {
            try
            {
                ExecutionResult executionResult = null;
                Transaction transaction = await _repository.GetSaleTransaction(
                    request.TransactionId,
                    integratorid
                );
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);

                if (transaction == null)
                {
                    executionResult = GenerateExecutionResult(
                        new BadRequestException("Transaction with specified ID was not found"),
                        API_MESSAGE_CONSTANTS.BAD_REQUEST
                    );
                    executionResult.status = "failed";
                    return Response
                        .WithStatus(executionResult.status)
                        .WithMessage(executionResult.failedResponse.ErrorMessage)
                        .WithType(executionResult)
                        .GenerateResponse();
                }
                else if (transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.status = "success";
                    executionResult.code = API_MESSAGE_CONSTANTS.OKAY_REQEUST;
                    executionResult.successResponse.UpdateResponseForStatusQuery(transaction);
                    return Response
                        .WithStatus(executionResult.status)
                        .WithMessage("Transaction Successfully fetched")
                        .WithType(executionResult)
                        .GenerateResponse();
                }
                else if (!transaction.Finalized)
                {
                    executionResult = await _executionContext.ExecuteTransaction(
                        transaction.VendtechTransactionID,
                        integratorid,
                        integratorName
                    );
                    if (executionResult.status == "success")
                    {
                        wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                        executionResult.successResponse.UpdateResponseForStatusQuery(transaction);
                        executionResult.code = API_MESSAGE_CONSTANTS.OKAY_REQEUST;
                        await _transactionUpdate.UpdateSuceessSaleTransactionLogOnStatusQuery(
                            executionResult,
                            transaction
                        );
                        transaction = await _repository.DeductFromWallet(
                            transactionId: transaction.Id,
                            walletId: wallet.Id
                        );
                        CheckIntegratorBalanceThreshold(wallet);
                        return Response
                            .WithStatus(executionResult.status)
                            .WithMessage("Transaction Successfully fetched")
                            .WithType(executionResult)
                            .GenerateResponse();
                    }
                    else if (executionResult.status == "failed")
                    {
                        if (executionResult.code == API_MESSAGE_CONSTANTS.VENDING_DISABLE)
                        {
                            await AppConfiguration.DisableSales();
                        }
                        transaction = await _repository.RefundToWallet(
                            transactionId: transaction.Id,
                            walletId: wallet.Id
                        );
                        await _transactionUpdate.UpdateFailedSaleTransactionLogOnStatusQuery(
                            executionResult,
                            transaction
                        );
                        return Response
                            .WithStatus(executionResult.status)
                            .WithMessage("Transaction unsuccessful")
                            .WithType(executionResult)
                            .GenerateResponse();
                    }
                }
                return Response
                    .WithStatus(executionResult.status)
                    .WithMessage(executionResult.failedResponse.ErrorMessage)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(
                    ex,
                    API_MESSAGE_CONSTANTS.BAD_REQUEST
                );
                return Response
                    .WithStatus("failed")
                    .WithMessage(ex.Message)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
            catch (SystemDisabledException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(
                    ex,
                    API_MESSAGE_CONSTANTS.VENDING_DISABLE
                );
                return Response
                    .WithStatus("failed")
                    .WithMessage(ex.Message)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
        }

        #endregion

        #region SANDBOX

        public async Task<APIResponse> PurchaseElectricityForSandbox(
            ElectricitySaleRequest request,
            Guid integratorid,
            string integratorName
        )
        {
            try
            {
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                await _repository.SalesInternalValidation(wallet, request, integratorid);

                ExecutionResult executionResult = null;
                Transaction existingTransaction = await _repository.GetSaleTransactionByRandom(
                    request
                );
                Transaction transaction = await _repository.CreateSaleTransactionLog(request, integratorid);
                transaction = await _repository.DeductFromWallet(transactionId: transaction.Id, walletId: wallet.Id);

                if (existingTransaction == null)
                {
                    executionResult = new ExecutionResult(isSuccessful: false);
                    executionResult.status = "failed";
                }
                else if (existingTransaction.TransactionStatus == (int)TransactionStatus.Success)
                {
                    executionResult = new ExecutionResult(existingTransaction,existingTransaction.ReceivedFrom);
                    executionResult.status = "success";
                    wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                    executionResult.successResponse.UpdateResponse(transaction, wallet);
                    executionResult.code = API_MESSAGE_CONSTANTS.OKAY_REQEUST;
                    await _transactionUpdate.UpdateSaleSuccessTransactionLogSANDBOX(existingTransaction, transaction);
                    CheckIntegratorBalanceThreshold(wallet);
                    return Response.WithStatus(executionResult.status)
                        .WithMessage("Vend successful")
                        .WithType(executionResult)
                        .GenerateResponse();
                }
                else if (existingTransaction.TransactionStatus == (int)TransactionStatus.Failed)
                {
                    executionResult = new ExecutionResult(existingTransaction, existingTransaction.ReceivedFrom);
                    executionResult.status = "failed";
                    executionResult.code = API_MESSAGE_CONSTANTS.BAD_REQUEST;
                    transaction = await _repository.RefundToWallet(
                        transactionId: transaction.Id,
                        walletId: wallet.Id
                    );
                    await _transactionUpdate.UpdateSaleFailedTransactionLog(
                        executionResult,
                        transaction
                    );
                    return Response
                        .WithStatus(executionResult.status)
                        .WithMessage(executionResult.failedResponse.ErrorMessage)
                        .WithType(executionResult)
                        .GenerateResponse();
                }
                else if (existingTransaction.TransactionStatus == (int)TransactionStatus.Pending)
                {
                    executionResult = new ExecutionResult(
                        existingTransaction,
                        existingTransaction.ReceivedFrom
                    );
                    executionResult.status = "pending";
                    executionResult.code = API_MESSAGE_CONSTANTS.BAD_REQUEST;
                    await _transactionUpdate.UpdateSalePendingTransactionLog(
                        executionResult,
                        transaction
                    );
                    return Response
                        .WithStatus(executionResult.status)
                        .WithMessage(executionResult.failedResponse.ErrorMessage)
                        .WithType(executionResult)
                        .GenerateResponse();
                }
                return Response
                    .WithStatus(executionResult.status)
                    .WithMessage("")
                    .WithType(executionResult)
                    .GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(
                    ex,
                    API_MESSAGE_CONSTANTS.BAD_REQUEST
                );
                return Response
                    .WithStatus("failed")
                    .WithMessage(ex.Message)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
            catch (SystemDisabledException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(
                    ex,
                    API_MESSAGE_CONSTANTS.VENDING_DISABLE
                );
                return Response
                    .WithStatus("failed")
                    .WithMessage(ex.Message)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
        }

        public async Task<APIResponse> QuerySalesStatusForSandbox(
            SaleStatusRequest request,
            Guid integratorid,
            string integratorName
        )
        {
            try
            {
                Wallet wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                ExecutionResult executionResult = null;
                Transaction transaction = await _repository.GetSaleTransaction(
                    request.TransactionId,
                    integratorid
                );

                if (transaction == null)
                {
                    executionResult = GenerateExecutionResult(
                        new BadRequestException("The specified transaction was not found"),
                        API_MESSAGE_CONSTANTS.BAD_REQUEST
                    );
                    return Response
                        .WithStatus("failed")
                        .WithMessage("Transaction not found")
                        .WithType(executionResult)
                        .GenerateResponse();
                }
                else if (transaction.Finalized && string.IsNullOrEmpty(transaction.Response))
                {
                    executionResult = GenerateExecutionResult(
                        new BadRequestException("No associated voucher"),
                        API_MESSAGE_CONSTANTS.BAD_REQUEST
                    );
                    return Response
                        .WithStatus("failed")
                        .WithMessage(executionResult.failedResponse.ErrorMessage)
                        .WithType(executionResult)
                        .GenerateResponse();
                }
                else if (transaction.Finalized)
                {
                    transaction = await _repository.DeductFromWallet(
                        transactionId: transaction.Id,
                        walletId: wallet.Id
                    );
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.status = "success";
                    wallet = await _walletReo.GetWalletByIntegratorId(integratorid);
                    executionResult.successResponse.UpdateResponse(transaction, wallet);
                    executionResult.code = API_MESSAGE_CONSTANTS.OKAY_REQEUST;
                }
                else if (!transaction.Finalized)
                {
                    executionResult = new ExecutionResult(transaction, transaction.ReceivedFrom);
                    executionResult.status = "pending";
                    executionResult.code = API_MESSAGE_CONSTANTS.REQUEST_PENDING;
                }
                return Response
                    .WithStatus(executionResult.status)
                    .WithMessage("")
                    .WithType(executionResult)
                    .GenerateResponse();
            }
            catch (BadRequestException ex)
            {
                ExecutionResult executionResult = GenerateExecutionResult(
                    ex,
                    API_MESSAGE_CONSTANTS.BAD_REQUEST
                );
                return Response
                    .WithStatus("failed")
                    .WithMessage(ex.Message)
                    .WithType(executionResult)
                    .GenerateResponse();
            }
        }

        #endregion
        private void AddSaleToQueue(
            string transactionId,
            string vtechTransactionId,
            Guid integratorId,
            string integratorName
        )
        {
            string jobId = $"{vtechTransactionId}_{integratorName}_{transactionId}";
            _recurringJobManager.AddOrUpdate(
                jobId,
                () =>
                    CheckPendingSalesStatus(
                        transactionId,
                        vtechTransactionId,
                        integratorId,
                        integratorName
                    ),
                Cron.Minutely
            );
            _recurringJobManager.Trigger(jobId);
        }

        public async Task CheckPendingSalesStatus(
            string transactionId,
            string vtechTransactionId,
            Guid integratorId,
            string integratorName
        )
        {
            string jobId = $"{vtechTransactionId}_{integratorName}_{transactionId}";
            try
            {
                Wallet wallet = null;
                Transaction transaction = await _repository.GetSaleTransaction(
                    transactionId,
                    integratorId
                );
                if (transaction == null)
                {
                    _logService.Log(
                        LogType.Error,
                        $"Transaction not found in CheckPendingSalesStatus: {transactionId}",
                        null
                    );
                    _recurringJobManager.RemoveIfExists(jobId);
                    return;
                }

                ExecutionResult executionResult = await _executionContext.ExecuteTransaction(
                    vtechTransactionId,
                    integratorId,
                    integratorName
                );

                if (executionResult.status == "success")
                {
                    _logService.Log(LogType.QeueJob, $"Removing job {jobId}", transaction);
                    _recurringJobManager.RemoveIfExists(jobId);
                    wallet = await _walletReo.GetWalletByIntegratorId(integratorId);
                    transaction = await _repository.DeductFromWalletIfRefunded(
                        transactionId: transaction.Id,
                        walletId: wallet.Id
                    );
                    transaction.ClaimedStatus = (int)ClaimedStatus.Unclaimed;
                    await _transactionUpdate.UpdateSuceessSaleTransactionLogOnStatusQuery(
                        executionResult,
                        transaction
                    );
                    return;
                }

                if (executionResult.status == "pending")
                {
                    _logService.Log(LogType.QeueJob, $"Re-Running job {jobId}", transaction);
                    return;
                }

                _logService.Log(LogType.QeueJob, $"Failed job {jobId}", transaction);
                wallet = await _walletReo.GetWalletByIntegratorId(integratorId);
                transaction = await _repository.RefundToWallet(
                    transactionId: transaction.Id,
                    walletId: wallet.Id
                );
                await _transactionUpdate.UpdateFailedSaleTransactionLogOnStatusQuery(
                    executionResult,
                    transaction
                );
                _recurringJobManager.RemoveIfExists(jobId);
            }
            catch (Exception ex)
            {
                _logService.Log(
                    LogType.Error,
                    $"Error in CheckPendingSalesStatus: {ex.Message}",
                    ex.ToString()
                );
                _recurringJobManager.RemoveIfExists(jobId);
            }
        }

        private ElectricitySaleRTO TransferRequestToRTO(
            ElectricitySaleRequest x,
            string vendtechTransactionId
        ) =>
            new ElectricitySaleRTO
            {
                Amount = x.Amount,
                MeterNumber = x.MeterNumber,
                TransactionId = x.TransactionId,
                VendtechTransactionId = vendtechTransactionId,
            };

        private void CheckIntegratorBalanceThreshold(Wallet wallet)
        {
            try
            {
                if (wallet.MinThreshold >= wallet.Balance)
                {
                    string jobId = "balance_low" + wallet.WALLET_ID;
                    if (!wallet.IsBalanceLowReminderSent)
                    {
                        _walletReo.UpdateBalanceLowReminder(
                            wallet.Id,
                            value: true,
                            walletId: wallet.WALLET_ID
                        );
                        _recurringJobManager.AddOrUpdate(
                            jobId,
                            () => new IntegratorBalanceJob().SendLowBalanceAlert(wallet.Id),
                            Cron.Daily
                        );
                        _recurringJobManager.Trigger(jobId);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
