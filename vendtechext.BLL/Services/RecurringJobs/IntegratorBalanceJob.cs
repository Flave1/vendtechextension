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
                    Thresholds thresholds = settings.Threshholds;
                    if (settings.Notification.LowBalance)
                    {

                        var wallets = await db.Wallets.Where(d => d.Deleted == false && d.Balance <= thresholds.MinimumDeposit)
                            .Include(d => d.Integrator).ThenInclude(d => d.AppUser).ToListAsync();
                        var notification = new NotificationHelper(db);
                        for (int i = 0; i < wallets.Count; i++)
                        {
                            new Emailer(new EmailHelper(DomainEnvironment.Configuration), notification).SendEmailToIntegratorOnBalanceLow(wallets[i], wallets[i].Integrator);
                        }
                    }

                }
            }
        }
    }
}
