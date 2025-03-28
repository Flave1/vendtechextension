﻿using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using vendtechext.BLL.Exceptions;
using vendtechext.BLL.Interfaces;
using vendtechext.BLL.Repository;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public class DepositService: BaseService, IDepositService
    {
        private readonly TransactionRepository _repository;
        private readonly WalletRepository _walletRepository;
        private readonly EmailHelper _emailHelper;
        private readonly IAuthService _authService;
        private readonly NotificationHelper notification;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public DepositService(
            TransactionRepository transactionRepository,
            WalletRepository walletRepository,
            EmailHelper emailHelper,
            IAuthService authService,
            NotificationHelper notification,
            IBackgroundJobClient backgroundJobClient)
        {
            _repository = transactionRepository;
            _walletRepository = walletRepository;
            _emailHelper = emailHelper;
            _authService = authService;
            this.notification = notification;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<APIResponse> CreateDeposit(DepositRequest request, Guid integratorid)
        {
            SettingsPayload settings = AppConfiguration.GetSettings();
            if (request.Amount <= 0)
            {
                throw new BadRequestException($"Amount is required.");
            }
            if (string.IsNullOrEmpty(request.Reference))
            {
                request.Reference = "0";
            }
            if (settings.Threshholds.MinimumDeposit > request.Amount)
            {
                throw new BadRequestException($"Deposit amount must not be less than {settings.Threshholds.MinimumDeposit}.");
            }




            var wallet = await _walletRepository.GetWalletByIntegratorId(integratorid, true);
            CreateDepositDto dto = new CreateDepositDto
            {
                Reference = request.Reference,
                BalanceBefore = wallet.Balance,
                Amount = request.Amount,
                BalanceAfter = wallet.Balance + request.Amount,
                IntegratorId = integratorid,
                PaymentTypeId = request.PaymentTypeId
            };
           
            Deposit deposit = await _repository.CreateDepositTransaction(dto, DepositStatus.Waiting);
            if(deposit != null) 
                await _walletRepository.UpdateWalletBookBalance(wallet, deposit.BalanceAfter);


            
            if (settings.Notification.SendAdminDepositEmail)
                _backgroundJobClient.Enqueue(() => CreateDepositNotification(wallet.WALLET_ID, deposit.Integrator.BusinessName, wallet.CommissionId, deposit.Amount, deposit.Id, deposit.CreatedAt));

            return Response.WithStatus("success").WithMessage("Successfully created deposit").WithType(request).GenerateResponse();
        }

        
        public async Task CreateDepositNotification(string WALLET_ID, string BusinessName, int CommissionId, decimal Amount, Guid DepositId, DateTime CreatedAt)
        {
            AppUser user = await _authService.FindAdminUser();
            new Emailer(_emailHelper, notification).SendEmailToAdminOnPendingDeposits(WALLET_ID, BusinessName, CommissionId, Amount, DepositId, CreatedAt, user);
        }

        public async Task<APIResponse> ApproveDeposit(ApproveDepositRequest request)
        {
            Deposit deposit = await _repository.GetDepositTransaction(request.DepositId);
            if (!request.Approve)
            {
                await _repository.DeleteDepositTransaction(deposit);
                long? notificationId = notification.GetNotificationId(request.DepositId.ToString());
                if (notificationId != null && notificationId.Value > 0)
                {
                    notification.UpdateNotificationReadStatus(notificationId.Value, request.ApprovingUserId);
                }
                return Response.WithStatus("success").WithMessage("Successfully Cancelled deposit").WithType(request).GenerateResponse();
            }

            Wallet wallet = await _walletRepository.GetWalletByIntegratorId(request.IntegratorId);
            await _repository.ApproveDepositTransaction(deposit);
            await _walletRepository.UpdateWalletBookBalance(wallet, deposit.BalanceAfter);
            await CreateCommision(deposit, request.IntegratorId, wallet);

            SettingsPayload settings = AppConfiguration.GetSettings();
            if (settings.Notification.SendDepositApprovalEmailToUser)
                _backgroundJobClient.Enqueue(() => ApproveDepositNotification(request.IntegratorId, deposit.Amount, deposit.Id, wallet.CommissionId, request.ApprovingUserId));

            return Response.WithStatus("success").WithMessage("Successfully approved deposit").WithType(request).GenerateResponse();
        }

        public async Task ApproveDepositNotification(Guid integratorId, decimal Amount, Guid DeposiId, int CommissionId, string currentAdminUserId)
        {
            long? notificationId = notification.GetNotificationId(DeposiId.ToString());
            if(notificationId != null && notificationId.Value > 0)
            {
                notification.UpdateNotificationReadStatus(notificationId.Value, currentAdminUserId);
            }
            AppUser user = await _authService.FindUserByIntegratorId(integratorId);
            new Emailer(_emailHelper, notification).SendEmailToIntegratorOnDepositApproval(Amount, DeposiId, CommissionId, user);
        }

        private async Task CreateCommision(Deposit deposit, Guid integratorid, Wallet wallet)
        {
            decimal commission = AppConfiguration.ProcessCommsion(deposit.Amount, wallet.CommissionId);
            PaymentTypeDto commissionMethod = await _repository.GetCommissionType();
            CreateDepositDto commsionDto = new CreateDepositDto
            {
                Reference = deposit.Reference,
                BalanceBefore = deposit.BalanceAfter,
                Amount = commission,
                BalanceAfter = deposit.BalanceAfter + commission,
                IntegratorId = integratorid,
                PaymentTypeId = commissionMethod.Id,
            };
            await _repository.CreateDepositTransaction(commsionDto, DepositStatus.Approved);
            await _walletRepository.UpdateWalletRealBalance(wallet, commsionDto.BalanceAfter);
        }

        public async Task<APIResponse> GetIntegratorDeposits(PaginatedSearchRequest req)
        {
            IQueryable<Deposit> query = _repository.GetDepositsQuery((DepositStatus)req.Status);

            query = FilterQuery(req, query);
            
            int totalRecords = await query.CountAsync();

            query = query.Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize);

            List<DepositDto> transactions = await query.Select(d => new DepositDto(d)).ToListAsync();

            PagedResponse<DepositDto> result = new PagedResponse<DepositDto>(transactions, totalRecords, req.PageNumber, req.PageSize);

            return Response.WithStatus("success").WithMessage("Successfully fetched deposits").WithType(result).GenerateResponse();
        }

        public async Task<APIResponse> GetPendingDeposits(PaginatedSearchRequest req)
        {
            IQueryable<Deposit> query = _repository.GetDepositsQuery(DepositStatus.Waiting);

            query = FilterQuery(req, query);

            int totalRecords = await query.CountAsync();

            query = query.Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize);

            List<DepositDto> transactions = await query.Select(d => new DepositDto(d)).ToListAsync();

            PagedResponse<DepositDto> result = new PagedResponse<DepositDto>(transactions, totalRecords, req.PageNumber, req.PageSize);
            return Response.WithStatus("success").WithMessage("Successfully fetched deposits").WithType(result).GenerateResponse();
        }

        private IQueryable<Deposit> FilterQuery(PaginatedSearchRequest req, IQueryable<Deposit> query)
        {

            if (req.IntegratorId != null)
                query = query.Where(d => d.IntegratorId == req.IntegratorId);

            if (!string.IsNullOrEmpty(req.From))
            {
                var fromDate = DateTime.Parse(req.From).Date;
                query = query.Where(p => p.CreatedAt.Date >= fromDate);
            }

            if (!string.IsNullOrEmpty(req.To))
            {
                var toDate = DateTime.Parse(req.To).Date;
                query = query.Where(p => p.CreatedAt.Date <= toDate);
            }

            if (Utils.IsAscending(req.SortOrder))
                query = query.OrderBy(d => d.CreatedAt);
            else
                query = query.OrderByDescending(d => d.CreatedAt);

            if (!string.IsNullOrEmpty(req.SortValue))
            {
                if (req.SortBy == "TRANSACTION_ID")
                    query = query.Where(d => d.TransactionId.Contains(req.SortValue));
                else if (req.SortBy == "AMOUNT")
                    query = query.Where(d => d.Amount.ToString().Contains(req.SortValue));
                else if (req.SortBy == "REFERENCE")
                    query = query.Where(d => d.Reference.Contains(req.SortValue));
                else if (req.SortBy == "WALLET_ID")
                    query = query.Where(d => d.Integrator.Wallet.WALLET_ID.Contains(req.SortValue));
                else if (req.SortBy == "INTEGRATOR")
                    query = query.Where(d => d.Integrator.BusinessName.Contains(req.SortValue));
            }
            return query;
        }

        public async Task<APIResponse> GetPaymentTypes()
        {
            var result = await _repository.GetPaymentTypes();
            return await Task.Run(() =>  Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse());
        }

        public async Task<APIResponse> GetWalletBalance(Guid integratorId, bool includeLastDeposit)
        {
            var wallet = await _walletRepository.GetWalletByIntegratorId(integratorId, true);
            List<LastDeposit> lastDeposit = null;
            if (includeLastDeposit)
            {
                lastDeposit = await _repository.GetLastDepositTransaction(integratorId);
            }
            var result = new WalletDTO
            {
                AccountName = wallet.Integrator.BusinessName,
                AccountNumber = wallet.WALLET_ID,
                BookBalance = wallet.BookBalance,
                WalletBalance = wallet.Balance,
                LastDeposit = lastDeposit,
                Logo = wallet.Integrator.Logo
            };
            return Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }

        public async Task<APIResponse> GetAdminBalance()
        {
            var balance = await _walletRepository.GetAdminBalance();
            var user = await _authService.FindAdminUser();
            var result = new WalletDTO
            {
                AccountName = "Administrator",
                AccountNumber = "VENDTECH SL",
                BookBalance = 0,
                WalletBalance = balance,
                LastDeposit = null,
                Logo = user.ProfilePic
            };
            return Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }

        public async Task<List<DepositExcelDto>> GetDepositReportForExportAsync(PaginatedSearchRequest req)
        {
            IQueryable<Deposit> query = _repository.GetDepositsQuery((DepositStatus)req.Status);

            query = FilterQuery(req, query);

            return await query.Select(d => new DepositExcelDto(d)).ToListAsync();
        }

        public APIResponse GetTodaysTransaction(Guid integratorId) {

            var result = _walletRepository.GetTodaysTransaction(integratorId);
            return Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }
        public APIResponse GetAdminTodaysTransaction()
        {
            var result = _walletRepository.GetAdminTodaysTransaction();
            return Response.WithStatus("success").WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }
    }
}
