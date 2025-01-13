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
                <p>Request Date: {Utils.formatDate(CreatedAt)}</p>
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
                //
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
