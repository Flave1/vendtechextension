﻿using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using vendtechext.BLL.Services;
using vendtechext.Contracts;
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
        public void SendEmailToAdminOnPendingDeposits(Deposit deposit, Wallet wallet, AppUser user)
        {
            try
            {
                string msg = $@"
                <p>This is to inform you that there is a deposit awaiting for your approval</p>
                <strong>Details:</strong>
                <p>Wallet ID: {wallet.WALLET_ID}</p>
                <p>Integrator: {wallet.Integrator.BusinessName}</p>
                <p>Amount: SLE {deposit.Amount}</p>
                <p>Request Date: {Utils.formatDate(deposit.CreatedAt)}</p>
                ";
                string subject = "PENDING DEPOSIT APPROVAL";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", user.FirstName);
                emailBody = emailBody.Replace("[body]", msg);

                notificationHelper.SaveNotification(subject, msg, user.Id, DAL.Common.NotificationType.DepositRequested, deposit.Id.ToString());
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SendEmailToIntegratorOnDepositApproval(Deposit deposit, Wallet wallet, AppUser user)
        {
            try
            {
                string msg = $@"
                <p>This is to inform you that your deposit of SLE: {deposit.Amount} has been approved</p>
                ";
                string subject = "PENDING DEPOSIT APPROVED";
                string emailBody = helper.GetEmailTemplate("simple");
                emailBody = emailBody.Replace("[recipient]", user.FirstName);
                emailBody = emailBody.Replace("[body]", msg);

                notificationHelper.SaveNotification(subject, msg, user.Id, DAL.Common.NotificationType.DepositApproved, deposit.Id.ToString());
                helper.SendEmail(user.Email, subject, emailBody);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
