using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using vendtechext.BLL.Services;
using vendtechext.Contracts;
using vendtechext.Contracts.VtchMainModels;
using vendtechext.DAL.Models;

namespace vendtechext.Helper
{
    public class EmailHelper
    {
        public readonly IConfiguration _configuration;
        public static bool SendNotification = true;
        private readonly string _dir;
        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _dir = "EmailTemplates";
        }

        public void SendEmail(List<string> to, string sub, string body)
        {
            if (!SendNotification)
                return;


            string displayName = _configuration["ClieEmailServicent:displayName"];
            var mimeMsg = new MimeMessage();
            var tos = new List<MailboxAddress>();
            to.ForEach(d =>
            {
                tos.Add(new MailboxAddress(displayName, d));
            });
            mimeMsg.To.AddRange(tos);
            mimeMsg.Subject = sub;

            mimeMsg.Body = new TextPart("html")
            {
                Text = body
            };
            Send(mimeMsg);
        }
        public void SendEmail(string to, string sub, string body)
        {
            if (!SendNotification)
                return;

            string displayName = _configuration["ClieEmailServicent:displayName"];
            var mimeMsg = new MimeMessage();
        
            var tos = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, to),
                };
            mimeMsg.To.AddRange(tos);
            mimeMsg.Subject = sub;

            mimeMsg.Body = new TextPart("html")
            {
                Text = body
            };
            Send(mimeMsg);
        }

        public string GetEmailTemplate(string template)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _dir, $"{template}.html");
            return File.ReadAllText(filePath);
        }

        void Send(MimeMessage mimeMsg)
        {
            try
            {
                string displayName = _configuration["ClieEmailServicent:displayName"];
                string from = _configuration["EmailService:from"];
                string password = _configuration["EmailService:password"];
                string smtp = _configuration["EmailService:smtp"];
                int port = Convert.ToInt16(_configuration["EmailService:port"]);
                var frms = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, from),
                };

                mimeMsg.From.AddRange(frms);
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                    try
                    {
                        client.Connect(smtp, port, SecureSocketOptions.StartTls);
                        client.Authenticate(from, password);
                    }
                    catch (Exception)
                    {
                        client.Connect("smtp.gmail.com", 465);
                        client.Authenticate("vtechsalone@gmail.com", "ozgrkqzcdtswxscl");
                    }
                    client.Send(mimeMsg);
                    client.Disconnect(true);

                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }


    public class Emailer
    {
        private readonly EmailHelper helper;
        public readonly NotificationHelper notificationHelper;
        public Emailer(EmailHelper helper, NotificationHelper notificationHelper)
        {
            this.helper = helper;
            this.notificationHelper = notificationHelper;
        }
        public void SendEmailToAdminOnPendingDeposits(string WALLET_ID, string BusinessName, int CommissionId, decimal Amount, Guid DepositId, DateTime CreatedAt, AppUser user)
        {
            try
            {
                decimal commission = AppConfiguration.ProcessCommsion(Amount, CommissionId);
                string msg = $@"
                <p>This is to inform you that there is a deposit awaiting for your approval</p>
                <strong>Details:</strong>
                <p>Wallet ID: {WALLET_ID}</p>
                <p>Integrator: {BusinessName}</p>
                <p>Amount: SLE {Utils.FormatAmount(Amount + commission)}</p>
                <p>request Date: {Utils.formatDate(CreatedAt)}</p>
                ";
                string subject = "PENDING DEPOSIT APPROVAL";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", user.FirstName);
                emailBody = emailBody.Replace("[body]", msg);

                notificationHelper.SaveNotification(subject, msg, user.Id, DAL.Common.NotificationType.DepositRequested, DepositId.ToString());
                //
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendEmailToIntegratorOnDepositApproval(decimal Amount, Guid DeposiId, int CommissionId, AppUser user)
        {
            try
            {
                decimal commission = AppConfiguration.ProcessCommsion(Amount, CommissionId);
                string msg = $@"
                <p>This is to inform you that your deposit of SLE: {Utils.FormatAmount(Amount)} has been approved</p>
                <strong>Details:</strong>
                <p>Amount: {Utils.FormatAmount(Amount)}</p>
                <p>Commission: SLE {Utils.FormatAmount(commission)}</p>
                <p>Total: {Utils.FormatAmount(Amount + commission)}</p>
                ";
                string subject = "PENDING DEPOSIT APPROVED";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", user.FirstName);
                emailBody = emailBody.Replace("[body]", msg);
                notificationHelper.SaveNotification(subject, msg, user.Id, DAL.Common.NotificationType.DepositApproved, DeposiId.ToString());
                //
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendReconcilationEmail(UserDetail user, TransactionDetail record)
        {
            try
            {
                string msg = $@"
                <p>This is to inform you that your account has been refunded with SLE: {Utils.FormatAmount(record.Amount)}.</p>
                <p>This is for the unsuccessful sale that happened on the {Utils.formatDate(record.CreatedAt)}.</p>
                <strong>Details:</strong>
                <p>Amount: {Utils.FormatAmount(record.Amount)}</p>
                <p>Transaction ID: {record.TransactionId}</p>
                <p>Date: {Utils.formatDate(record.CreatedAt)}</p>
                ";
                string subject = $"BALANCE REFUND {record.Amount}";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", user.FirstName);
                emailBody = emailBody.Replace("[body]", msg);
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendEmailToIntegratorOnAccountCreation(Integrator integrator, AppUser user)
        {
            try
            {
                string subject = $"VENDTECH API CREDENTIALS";
                string emailBody = helper.GetEmailTemplate("new_integrator");
                emailBody = emailBody.Replace("[apikey]", integrator.ApiKey);
                emailBody = emailBody.Replace("[api_url]", DomainEnvironment.APIUrl);
                emailBody = emailBody.Replace("[dashboard_url]", DomainEnvironment.DashboardUrl);
                emailBody = emailBody.Replace("[username]", user.Email);
                emailBody = emailBody.Replace("[password]", CREDENTIALS.INTEGRATOR_PASSWORD);
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }
        public void SendEmailForPasswordResetLink(AppUser user, string callbackUrl)
        {
            try
            {
                string subject = "Change Password Link";
                string emailBody = helper.GetEmailTemplate("password_reset");
                emailBody = emailBody.Replace("[reset_link]", callbackUrl);
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendEmailOnPasswordResetSuccess(AppUser user, string body)
        {
            try
            {
                string subject = "Password Changed successfully";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", user.FirstName);
                emailBody = emailBody.Replace("[body]", body);
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendEmailToIntegratorOnBalanceLow(Wallet wallet, Integrator integrator)
        {
            try
            {
                string subject = "VENDTECH SUPPORT | WALLET BALANCE LOW NOTIFICATION";
                string emailBody = helper.GetEmailTemplate("balance_low");

                emailBody = emailBody.Replace("[BusinessName]", integrator.BusinessName);
                emailBody = emailBody.Replace("[Date]", Utils.formatDate(DateTime.UtcNow).Split(" ")[0]);
                emailBody = emailBody.Replace("[Time]", Utils.formatDate(DateTime.UtcNow).Split(" ")[1]);
                emailBody = emailBody.Replace("[Balance]", Utils.FormatAmount(wallet.Balance));
                emailBody = emailBody.Replace("[fund_wallet_link]", $"{DomainEnvironment.DashboardUrl}/deposit_form");
                notificationHelper.SaveNotification(subject, emailBody, integrator.AppUser.Id, DAL.Common.NotificationType.DepositApproved, integrator.Id.ToString());

                helper.SendEmail("favouremmanuel433@gmail.com", subject, emailBody);
                helper.SendEmail(integrator.AppUser.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }
        public void SendEmailToIntegratorOnBalanceAlert(Wallet wallet, Integrator integrator)
        {
            try
            {
                string subject = "Integrator Balance";
                string emailBody = helper.GetEmailTemplate("midnight_balance");

                emailBody = emailBody.Replace("[BusinessName]", integrator.BusinessName);
                emailBody = emailBody.Replace("[Date]", Utils.formatDate(DateTime.UtcNow).Split(" ")[0]);
                emailBody = emailBody.Replace("[Time]", Utils.formatDate(DateTime.UtcNow).Split(" ")[1]);
                emailBody = emailBody.Replace("[Balance]", Utils.FormatAmount(wallet.Balance));
                emailBody = emailBody.Replace("[fund_wallet_link]", $"{DomainEnvironment.DashboardUrl}/deposit_form");
                emailBody = emailBody.Replace("[email_setting_link]", $"{DomainEnvironment.DashboardUrl}/edit-profile ");
                notificationHelper.SaveNotification(subject, emailBody, integrator.AppUser.Id, DAL.Common.NotificationType.DepositApproved, integrator.Id.ToString());

                helper.SendEmail("favouremmanuel433@gmail.com", subject, emailBody);
                helper.SendEmail(integrator.AppUser.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }


        public void SendApiKeyGenerationEmail(Integrator integrator, string key)
        {
            try
            {
                string msg = $@"
        <p>A new API key has been successfully generated for your account. Please review the details below:</p>
        <p><strong>API Key:</strong> <code>{key}</code></p>
        <p><strong>Important Information:</strong></p>
        <ul>
            <li><strong>Activation Required:</strong> This API key is <strong> not yet active.</strong> Ensure you complete the necessary steps to activate it.</li>
            <li>If you did not request this key or suspect any unauthorized activity, please contact our support team immediately.</li>
        </ul>
        <p>Once you are ready to go live, integrate this key into your system accordingly.</p>
        ";

                string subject = "Action Required: Your New API Key Has Been Generated";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", integrator.BusinessName);
                emailBody = emailBody.Replace("[body]", msg);
                helper.SendEmail(integrator.AppUser.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }
        public void SendApiKeyAssociationConfirmationEmail(Integrator integrator, string key)
        {
            try
            {
                string msg = $@"
        <p>Your API key has been successfully associated with your account and is <strong>now ready for use</strong>.</p>
        <p><strong>API Key:</strong> <code>{key}</code></p>
        <p><strong>Next Steps:</strong></p>
        <ul>
            <li>Ensure your system is correctly configured to use this API key.</li>
            <li>If you experience any issues or did not authorize this change, contact our support team immediately.</li>
        </ul>
        ";

                string subject = "Confirmation: Your API Key Has Been Successfully Associated";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", integrator.BusinessName);
                emailBody = emailBody.Replace("[body]", msg);
                helper.SendEmail(integrator.AppUser.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }


    }
}
