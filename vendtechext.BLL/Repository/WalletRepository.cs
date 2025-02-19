using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
        private readonly IConfiguration _configuration;

        public WalletRepository(DataContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<Wallet> CreateWallet(Guid integratorId, int CommissionLevel)
        {
            var wallet = new WalletBuilder()
                .SetWalletId(UniqueIDGenerator.GenerateAccountNumber("000"))
                .SetCommission(CommissionLevel)
                .SetIntegratorId(integratorId)
                .SetBalance(0)
                .Build();

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        //public async Task<Wallet> GetWalletByIntegratorId(Guid integratorId, bool includeIntegrator = false)
        //{
        //    Wallet wallet = null;
        //    if(!includeIntegrator)
        //        wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.IntegratorId == integratorId);
        //    else
        //        wallet = await _context.Wallets.Where(w => w.IntegratorId == integratorId).Include(g => g.Integrator).FirstOrDefaultAsync();
        //    if (wallet == null)
        //    {
        //        throw new BadRequestException("Unable to find integrator wallet");
        //    }
        //    return wallet;
        //}

        public async Task<Wallet> GetWalletByIntegratorId(Guid integratorId, bool includeIntegrator = false)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    if (!includeIntegrator)
                    {
                        // Query wallet without Integrator details
                        command.CommandText = @"
                    SELECT Id, WALLET_ID, Balance, BookBalance, CommissionId, IntegratorId 
                    FROM Wallets WHERE IntegratorId = @integratorId;";
                    }
                    else
                    {
                        // Query wallet with Integrator details
                        command.CommandText = @"
                    SELECT w.Id, w.WALLET_ID, w.Balance, w.BookBalance, w.CommissionId, w.IntegratorId, 
                           i.Id as IntegratorId, i.AppUserId, i.BusinessName, i.About, i.Logo, i.Disabled, i.ApiKey
                    FROM Wallets w
                    INNER JOIN Integrators i ON w.IntegratorId = i.Id
                    WHERE w.IntegratorId = @integratorId;";
                    }

                    var integratorIdParam = command.CreateParameter();
                    integratorIdParam.ParameterName = "@integratorId";
                    integratorIdParam.Value = integratorId;
                    command.Parameters.Add(integratorIdParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            throw new ServerTechnicalException("Unable to find integrator wallet");
                        }

                        var wallet = new Wallet
                        {
                            Id = reader.GetGuid(0),
                            WALLET_ID = reader.GetString(1),
                            Balance = reader.GetDecimal(2),
                            BookBalance = reader.GetDecimal(3),
                            CommissionId = reader.GetInt32(4),
                            IntegratorId = reader.GetGuid(5),
                        };

                        if (includeIntegrator && reader.FieldCount > 6)
                        {
                            wallet.Integrator = new Integrator
                            {
                                Id = reader.GetGuid(6),
                                AppUserId = reader.GetString(7),
                                BusinessName = reader.GetString(8),
                                About = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Logo = reader.IsDBNull(10) ? null : reader.GetString(10),
                                Disabled = reader.GetBoolean(11),
                                ApiKey = reader.GetString(12)
                            };
                        }

                        return wallet;
                    }
                }
            }
        }


        public async Task<decimal> GetAdminBalance()
        {
            Transaction lastTransaction = await _context.Transactions
                .Where(d => d.Deleted == false && d.TransactionStatus == (int)TransactionStatus.Success)
                .OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            if(lastTransaction != null)
            {
                return lastTransaction.SellerReturnedBalance - lastTransaction.Amount;
            }
            return 0;
        }


        public async Task UpdateWalletRealBalance(Wallet wallet, decimal newBalance)
        {
            new WalletBuilder(wallet).SetBalance(newBalance).Build();
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
            res.Deposits = _context.Deposits.Where(d => d.Deleted == false && d.IntegratorId == integratorId && d.CreatedAt.Date == todaysDate && d.Status == (int)DepositStatus.Approved).Sum(g => g.Amount);
            res.Sales = _context.Transactions.Where(d => d.Deleted == false && d.IntegratorId == integratorId && d.TransactionStatus == (int)TransactionStatus.Success && d.CreatedAt.Date == todaysDate).Sum(g => g.Amount);

            return res;
        }

        public TodaysTransaction GetAdminTodaysTransaction()
        {
            var todaysDate = DateTime.UtcNow.Date;

            var res = new TodaysTransaction();
            res.Deposits = _context.Deposits.Where(d => d.Deleted == false && d.CreatedAt.Date == todaysDate && d.Status == (int)DepositStatus.Approved).Sum(g => g.Amount);
            res.Sales = _context.Transactions.Where(d => d.Deleted == false && d.TransactionStatus == (int)TransactionStatus.Success && d.CreatedAt.Date == todaysDate).Sum(g => g.Amount);

            return res;
        }
    }
}
