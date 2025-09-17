using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using vendtechext.Contracts;
using vendtechext.Contracts.VtchMainModels;
using vendtechext.DAL.Common;
using vendtechext.DAL.Migrations;
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
            try
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
            catch (Exception ex)
            {
                using (var db= new DataContext())
                {
                    new LogService(db).Log(LogType.Error, ex.Message, ex);
                }
            }
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

    public static class GenerateContent
    {
        public static (string, string) EmailToAdminOnPendingDeposits(string WALLET_ID, string BusinessName, DateTime CreatedAt, int CommissionId, decimal Amount)
        {
            decimal commission = AppConfiguration.ProcessCommsion(Amount, CommissionId);
            string msg = $@"
                <p>This is to inform you that there is a deposit awaiting for your approval</p>
                <strong>Details:</strong>
                <p>Wallet ID: {WALLET_ID}</p>
                <p>Integrator: 
                {BusinessName}</p>
                <p>Amount: SLE {Utils.FormatAmount(Amount + commission)}</p>
                <p>request Date: {Utils.formatDate(CreatedAt)}</p>
                ";
            string subject = "PENDING DEPOSIT APPROVAL";
            return (subject, msg);
        }

        public static (string, string) EmailToIntegratorOnDepositApproval(decimal Amount, Guid DeposiId, int CommissionId)
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
            return (subject, msg);
        }

        public static (string, string) EmailToIntegratorOnBalanceAlert(Wallet wallet, string emailBody)
        {
            string subject = "Integrator Balance";
            emailBody = emailBody.Replace("[BusinessName]", wallet.Integrator.BusinessName);
            emailBody = emailBody.Replace("[Date]", Utils.formatDate(DateTime.UtcNow).Split(" ")[0]);
            emailBody = emailBody.Replace("[Time]", Utils.formatDate(DateTime.UtcNow).Split(" ")[1]);
            emailBody = emailBody.Replace("[Balance]", Utils.FormatAmount(wallet.Balance));
            emailBody = emailBody.Replace("[fund_wallet_link]", $"{DomainEnvironment.DashboardUrl}/deposit_form");
            emailBody = emailBody.Replace("[email_setting_link]", $"{DomainEnvironment.DashboardUrl}/edit-profile ");
            return (subject, emailBody);
        }

        public static (string, string) EmailToIntegratorOnBalanceLow(Wallet wallet, string emailBody)
        {
            string subject = "VENDTECH SUPPORT | WALLET BALANCE LOW NOTIFICATION";
            emailBody = emailBody.Replace("[BusinessName]", wallet.Integrator.BusinessName);
            emailBody = emailBody.Replace("[Date]", Utils.formatDate(DateTime.UtcNow).Split(" ")[0]);
            emailBody = emailBody.Replace("[Time]", Utils.formatDate(DateTime.UtcNow).Split(" ")[1]);
            emailBody = emailBody.Replace("[Balance]", Utils.FormatAmount(wallet.Balance));
            emailBody = emailBody.Replace("[fund_wallet_link]", $"{DomainEnvironment.DashboardUrl}/deposit_form");
            return (subject, emailBody);
        }
    }

    public class Emailer
    {
        private readonly EmailHelper helper;
        public Emailer(EmailHelper helper)
        {
            this.helper = helper;
        }

        private void Log(Exception ex)
        {
            using (var db = new DataContext())
            {
                new LogService(db).Log(LogType.Error, ex.Message, ex);
            }
        }
        public void SendEmailToAdminOnPendingDeposits(NotificationRequest request)
        {
            try
            {
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", request.FirstName);
                emailBody = emailBody.Replace("[body]", request.Message);
                helper.SendEmail(request.Email, request.Subject, emailBody);
            }
            catch (Exception ex)
            {
                Log(ex);
                return;
            }
        }

        public void SendEmailToIntegratorOnDepositApproval(NotificationRequest request)
        {
            try
            {
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", request.FirstName);
                emailBody = emailBody.Replace("[body]", request.Message);
                helper.SendEmail(request.Email, request.Subject, emailBody);
            }
            catch (Exception ex)
            {
                Log(ex);
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
            catch (Exception ex)
            {
                Log(ex);
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
            catch (Exception ex)
            {
                Log(ex);
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
            catch (Exception ex)
            {
                Log(ex);
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
            catch (Exception ex)
            {
                Log(ex);
                return;
            }
        }

        public void SendEmailForPinRecovery(AppUser user, string body)
        {
            try
            {
                string subject = "PIN Recovery Token";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", user.FirstName);
                emailBody = emailBody.Replace("[body]", body);
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Log(ex);
                return;
            }
        }

        public void SendEmailToIntegratorOnBalanceLow(NotificationRequest request)
        {
            try
            {
                helper.SendEmail("favouremmanuel433@gmail.com", request.Subject, request.Message);
                helper.SendEmail(request.Email, request.Subject, request.Message);
            }
            catch (Exception ex)
            {
                Log(ex);
                return;
            }
        }
        public void SendEmailToIntegratorOnBalanceAlert(NotificationRequest request)
        {
            try
            {
                helper.SendEmail("favouremmanuel433@gmail.com", request.Subject, request.Message);
                helper.SendEmail(request.Email, request.Subject, request.Message);
            }
            catch (Exception ex)
            {
                Log(ex);
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
            catch (Exception ex)
            {
                Log(ex);
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
            catch (Exception ex)
            {
                Log(ex);
                return;
            }
        }

        public void SendSimpleEmail(NotificationRequest request)
        {
            try
            {
                string subject = request.Subject;
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", request.FirstName);
                emailBody = emailBody.Replace("[body]", request.EmailMessage);
                helper.SendEmail(request.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Log(ex);
                return;
            }
        }

    }
}
