using vendtechext.BLL.Common;
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
        public DepositService(
            TransactionRepository transactionRepository, 
            WalletRepository walletRepository)
        {
            _repository = transactionRepository;
            _walletRepository = walletRepository;
        }

        public async Task<APIResponse> CreateDeposit(DepositRequest request, Guid integratorid)
        {
            var wallet = await _walletRepository.GetWalletByIntegratorId(integratorid);
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
            
            await ApproveDeposit(new ApproveDepositRequest { DepositId = deposit.Id, IntegratorId = integratorid });

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully created deposit").WithType(request).GenerateResponse();
        }

        public async Task<APIResponse> ApproveDeposit(ApproveDepositRequest request)
        {
            var wallet = await _walletRepository.GetWalletByIntegratorId(request.IntegratorId);
 
            Deposit deposit = await _repository.GetDepositTransaction(request.DepositId);

            await _repository.ApproveDepositTransaction(deposit);
            await _walletRepository.UpdateWalletBookBalance(wallet, deposit.BalanceAfter);
            await CreateCommision(deposit, request.IntegratorId, wallet);

            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully approved deposit").WithType(request).GenerateResponse();
        }

        private async Task CreateCommision(Deposit deposit, Guid integratorid, Wallet wallet)
        {
            decimal.TryParse("0.5", out decimal percentage);

            decimal commission = deposit.Amount * percentage / 100;

            CreateDepositDto commsionDto = new CreateDepositDto
            {
                Reference = deposit.Reference,
                BalanceBefore = wallet.Balance,
                Amount = commission,
                BalanceAfter = wallet.Balance + commission,
                IntegratorId = integratorid
            };
            await _repository.CreateDepositTransaction(commsionDto, DepositStatus.Approved);
            await _walletRepository.UpdateWalletRealBalance(wallet, deposit.BalanceAfter);
        }

        public async Task<APIResponse> GetIntegratorDeposits(Guid integratorId)
        {
            List<DepositDto> result = await _repository.GetDeposits(integratorId, DepositStatus.Approved);
            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully fetched deposits").WithType(result).GenerateResponse();
        }

        public async Task<APIResponse> GetPaymentTypes()
        {
            var result = await _repository.GetPaymentTypes();
            return await Task.Run(() =>  Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully fetched").WithType(result).GenerateResponse());
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
            };
            return Response.WithStatus("success").WithStatusCode(200).WithMessage("Successfully fetched").WithType(result).GenerateResponse();
        }


    }
}
