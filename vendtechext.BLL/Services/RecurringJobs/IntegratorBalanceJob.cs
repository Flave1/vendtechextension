using Hangfire;
using Microsoft.EntityFrameworkCore;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Migrations;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services.RecurringJobs
{
    public class IntegratorBalanceJob
    {
        //not tested
        public async Task RunMidnight()
        {
            if (DomainEnvironment.IsProduction)
            {
                using (DataContext db = new DataContext())
                {
                    SettingsPayload settings = AppConfiguration.GetSettings();
                    if (settings.Notification.MidnightBalanceAlert)
                    {
                        var wallets = await db.Wallets.Where(d => d.Deleted == false)
                            .Include(d => d.Integrator)
                            .ThenInclude(d => d.AppUser).ToListAsync();

                        var channels = new List<INotificationChannel>
                        {
                            new EmailNotificationChannel(),
                            new DatabaseNotificationChannel()
                        };
                        var helper = new EmailHelper(DomainEnvironment.Configuration);
                        string emailBody = helper.GetEmailTemplate("midnight_balance");   
                        var notification = new NotificationService(channels);
                        for (int i = 0; i < wallets.Count; i++)
                        {
                            Wallet wallet= wallets[i];
                            var msg = GenerateContent.EmailToIntegratorOnBalanceAlert(wallet, emailBody);
                            var notRequest = new NotificationRequest
                            {
                                Email = wallet.Integrator.AppUser.Email,
                                FirstName = wallet.Integrator.AppUser.FirstName,
                                TargetId = wallet.Integrator.Id.ToString(),
                                Subject = msg.Item1,
                                Message = msg.Item2,
                                Emailtype = EmailTypeEnum.SendEmailToIntegratorOnBalanceAlert,
                                NotificationType = NotificationType.MidNightBalanceAlert,
                                SendEmail = true,
                                SaveToDatabase = true,
                                SendPush = false
                            };

                            if (wallets[i].MidnightBalanceAlertSwitch == (int)SwitchEnum.ON 
                                && wallets[i].Integrator.AppUser.UserAccountStatus == (int)UserAccountStatus.Active)
                            {
                                notification.SendNotification(notRequest);
                            }
                        }
                    }

                }
            }
        }

        //not tested
        public async Task SendLowBalanceAlert(Guid id)
        {
            if (DomainEnvironment.IsProduction)
            {
                using (DataContext db = new DataContext())
                {
                    SettingsPayload settings = AppConfiguration.GetSettings();
                    if (settings.Notification.LowBalance)
                    {
                        Wallet wallet = await db.Wallets.Where(d => d.Deleted == false && d.Id == id)
                            .Include(d => d.Integrator)
                            .ThenInclude(d => d.AppUser).FirstOrDefaultAsync();

                        var channels = new List<INotificationChannel>
                        {
                            new EmailNotificationChannel(),
                            new DatabaseNotificationChannel()
                        };

                        if (wallet.Integrator.AppUser.UserAccountStatus == (int)UserAccountStatus.Active)
                        {
                            var notification = new NotificationService(channels);                           
                            var helper = new EmailHelper(DomainEnvironment.Configuration);
                            string emailBody = helper.GetEmailTemplate("balance_low");
                            var msg = GenerateContent.EmailToIntegratorOnBalanceLow(wallet, emailBody);

                            var notRequest = new NotificationRequest
                            {
                                Email = wallet.Integrator.AppUser.Email,
                                FirstName = wallet.Integrator.AppUser.FirstName,
                                TargetId = wallet.Integrator.Id.ToString(),
                                Subject = msg.Item1,
                                Message = msg.Item2,
                                Emailtype = EmailTypeEnum.SendEmailToIntegratorOnBalanceLow,
                                NotificationType = NotificationType.BalanceLowAlert,
                                SendEmail = true,
                                SaveToDatabase = true,
                                SendPush = false
                            };

                            notification.SendNotification(notRequest);
                        }
                        else
                        {
                            string jobId = "BALANCE_LOW_" + wallet.WALLET_ID;
                            RecurringJob.RemoveIfExists(jobId);
                        }
                    }

                }
            }
        }
    }
}
