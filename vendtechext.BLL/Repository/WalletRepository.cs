using Hangfire;
using Hangfire.Server;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public async Task<Wallet> CreateWallet(Guid integratorId, int CommissionLevel, int minThreshold)
        {
            var wallet = new WalletBuilder()
                .SetWalletId(UniqueIDGenerator.GenerateAccountNumber("000"))
                .WithMinThreshold(minThreshold)
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
                    command.CommandText = includeIntegrator
                        ? @"SELECT w.Id, w.WALLET_ID, w.Balance, w.BookBalance, w.CommissionId, w.IntegratorId, w.MinThreshold, w.IsBalanceLowReminderSent,
                         i.Id as IntegratorId, i.AppUserId, i.BusinessName, i.About, i.Logo, i.Disabled, i.ApiKey
                    FROM Wallets w
                    LEFT JOIN Integrators i ON w.IntegratorId = i.Id
                    WHERE w.IntegratorId = @integratorId;"
                        : @"SELECT Id, WALLET_ID, Balance, BookBalance, CommissionId, IntegratorId, MinThreshold, IsBalanceLowReminderSent
                    FROM Wallets WHERE IntegratorId = @integratorId;";

                    command.Parameters.AddWithValue("@integratorId", integratorId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            throw new ServerTechnicalException("Unable to find integrator wallet");
                        }

                        var wallet = new Wallet
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("Id")),
                            WALLET_ID = reader.GetString(reader.GetOrdinal("WALLET_ID")),
                            Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
                            BookBalance = reader.GetDecimal(reader.GetOrdinal("BookBalance")),
                            CommissionId = reader.IsDBNull(reader.GetOrdinal("CommissionId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CommissionId")),
                            IntegratorId = reader.IsDBNull(reader.GetOrdinal("IntegratorId")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("IntegratorId")),
                            MinThreshold = reader.GetInt32(reader.GetOrdinal("MinThreshold")),
                            IsBalanceLowReminderSent = reader.GetBoolean(reader.GetOrdinal("IsBalanceLowReminderSent"))
                        };

                        if (includeIntegrator && reader.FieldCount > 7)
                        {
                            wallet.Integrator = new Integrator
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("IntegratorId")),
                                AppUserId = reader.GetString(reader.GetOrdinal("AppUserId")),
                                BusinessName = reader.GetString(reader.GetOrdinal("BusinessName")),
                                About = reader.IsDBNull(reader.GetOrdinal("About")) ? null : reader.GetString(reader.GetOrdinal("About")),
                                Logo = reader.IsDBNull(reader.GetOrdinal("Logo")) ? null : reader.GetString(reader.GetOrdinal("Logo")),
                                Disabled = reader.GetBoolean(reader.GetOrdinal("Disabled")),
                                ApiKey = reader.GetString(reader.GetOrdinal("ApiKey"))
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


        public async Task UpdateWalletRealBalance(Guid walletId, decimal newBalance)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Wallets SET Balance = Balance + @p0 WHERE Id = @p1",
                parameters: new[] { newBalance.ToString(), walletId.ToString() });
        }

        public void UpdateIsBalanceLowReminderSent(Guid id, bool value, string walletId)
        {
            int sent = 0;
            if (value)
            {
                sent = 1;
            }
            else
            {
                sent = 0;
            }

            if(!value)
                RecurringJob.RemoveIfExists($"balance_low{walletId}");

             _context.Database.ExecuteSqlRaw(
                "UPDATE Wallets SET IsBalanceLowReminderSent = @p0 WHERE Id = @p1",
                parameters: new[] { sent.ToString(), id.ToString() });
        }
        public async Task UpdateWalletBookBalance(Wallet wallet, decimal newBalance)
        {
            wallet = new WalletBuilder(wallet)
                .SetBookBalance(newBalance)
                .Build();

            await _context.SaveChangesAsync();
        }


        public async Task MarkWalletAsDeleted(Wallet wallet)
        {
            wallet = new WalletBuilder(wallet)
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
