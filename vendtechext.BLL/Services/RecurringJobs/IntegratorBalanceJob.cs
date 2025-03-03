using Microsoft.EntityFrameworkCore;
using vendtechext.Contracts;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services.RecurringJobs
{
    public class IntegratorBalanceJob
    {
        public async Task Run()
        {
            if (DomainEnvironment.IsSandbox)
            {
                using (DataContext db = new DataContext())
                {
                    SettingsPayload settings = AppConfiguration.GetSettings();
                    if (settings.Notification.MidnightBalanceAlert)
                    {
                        var wallets = await db.Wallets.Where(d => d.Deleted == false)
                            .Include(d => d.Integrator)
                            .ThenInclude(d => d.AppUser).ToListAsync();

                        var notification = new NotificationHelper(db);
                        for (int i = 0; i < wallets.Count; i++)
                        {
                            new Emailer(new EmailHelper(DomainEnvironment.Configuration), notification).SendEmailToIntegratorOnBalanceAlert(wallets[i], wallets[i].Integrator);
                        }
                    }

                }
            }
        }

        public async Task SendLowBalanceAlert(Guid id)
        {
            if (DomainEnvironment.IsSandbox)
            {
                using (DataContext db = new DataContext())
                {
                    SettingsPayload settings = AppConfiguration.GetSettings();
                    if (settings.Notification.LowBalance)
                    {
                        Wallet wallet = await db.Wallets.Where(d => d.Deleted == false && d.Id == id)
                            .Include(d => d.Integrator)
                            .ThenInclude(d => d.AppUser).FirstOrDefaultAsync();

                        var notification = new NotificationHelper(db);
                        await Task.Run(() => new Emailer(new EmailHelper(DomainEnvironment.Configuration), notification).SendEmailToIntegratorOnBalanceLow(wallet, wallet.Integrator));
                    }

                }
            }
        }
    }
}
