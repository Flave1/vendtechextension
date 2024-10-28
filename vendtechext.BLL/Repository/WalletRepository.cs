using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Common;
using vendtechext.BLL.Exceptions;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.DomainBuilders;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Repository
{
    public class WalletRepository
    {
        private readonly DataContext _context;

        public WalletRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Wallet> CreateWallet(Guid integratorId)
        {
            var wallet = new WalletBuilder()
                .SetIntegratorId(integratorId)
                .SetWalletId(UniqueIDGenerator.GenerateAccountNumber("000"))
                .SetBalanceBefore(0)
                .SetBalance(0)
                .Build();

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<Wallet> GetWalletByIntegratorId(Guid integratorId, bool includeIntegrator = false)
        {
            Wallet wallet = null;
            if(!includeIntegrator)
                wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.IntegratorId == integratorId);
            else
                wallet = await _context.Wallets.Where(w => w.IntegratorId == integratorId).Include(g => g.Integrator).FirstOrDefaultAsync();
            if (wallet == null)
            {
                throw new BadRequestException("Unable to find integrator wallet");
            }
            return wallet;
        }

        public async Task<decimal> GetAdminBalance() => await _context.Wallets.Where(d => d.Deleted == false).SumAsync(d => d.Balance);


        public async Task UpdateWalletRealBalance(Wallet wallet, decimal newBalance)
        {
            new WalletBuilder(wallet)
                .SetBalanceBefore(wallet.Balance)
                .SetBalance(newBalance)
                .Build();

            await _context.SaveChangesAsync();
        }
        public async Task UpdateWalletBookBalance(Wallet wallet, decimal newBalance)
        {
            new WalletBuilder(wallet)
                .SetBookBalance(newBalance)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task MarkWalletAsDeleted(Wallet wallet)
        {
            new WalletBuilder(wallet)
                .SetDeleted(true)
                .SetUpdatedAt(DateTime.Now)
                .Build();

            await _context.SaveChangesAsync();
        }

        public async Task<bool> WalletExists(Guid integratorId)
        {
            return await _context.Wallets.AnyAsync(w => w.IntegratorId == integratorId && !w.Deleted);
        }
        public TodaysTransaction GetTodaysTransaction(Guid integratorId)
        {
            var todaysDate = DateTime.UtcNow.Date;

            var res = new TodaysTransaction();
            res.Deposits = _context.Deposits.FirstOrDefault(d => d.Deleted == false && d.IntegratorId == integratorId && d.CreatedAt.Date == todaysDate && d.Status == (int)DepositStatus.Approved)?.Amount ?? 0;
            res.Sales = _context.Transactions.FirstOrDefault(d => d.Deleted == false && d.IntegratorId == integratorId && d.TransactionStatus == (int)DepositStatus.Approved && d.CreatedAt.Date == todaysDate)?.Amount ?? 0;

            return res;
        }

        public TodaysTransaction GetAdminTodaysTransaction()
        {
            var todaysDate = DateTime.UtcNow.Date;

            var res = new TodaysTransaction();
            res.Deposits = _context.Transactions.FirstOrDefault(d => d.Deleted == false && d.TransactionStatus == (int)TransactionStatus.Success && d.CreatedAt.Date == todaysDate)?.Amount ?? 0;
            res.Sales = _context.Wallets.FirstOrDefault(d => d.Deleted == false && d.CreatedAt.Date == todaysDate)?.Balance ?? 0;

            return res;
        }
    }
}
