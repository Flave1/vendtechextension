using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace vendtechext.Helper
{
    public class EmailHelper
    {
        public readonly IConfiguration _configuration;
        public static bool SendNotification = true;
        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string to, string sub, string body)
        {
            if (!SendNotification)
                return;
            string from = _configuration["EmailService:from"];
            string password = _configuration["EmailService:password"];
            string displayName = _configuration["ClieEmailServicent:displayName"];
            string smtp = _configuration["EmailService:smtp"];
            int port = Convert.ToInt16(_configuration["EmailService:port"]);
            try
            {
                var mimeMsg = new MimeMessage();
                var frms = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, from),
                };
                var tos = new List<MailboxAddress>
                {
                     new MailboxAddress(displayName, to),
                };
                mimeMsg.From.AddRange(frms);
                mimeMsg.To.AddRange(tos);
                mimeMsg.Subject = sub;

                mimeMsg.Body = new TextPart("html")
                {
                    Text = body
                };

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
}
