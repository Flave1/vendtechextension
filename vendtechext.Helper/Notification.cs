using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using vendtechext.SDK.HubConnection;
using vendtechext.SDK.Models;
using vendtechext.DAL.Models;
using vendtechext.SDK;
using vendtechext.Contracts;

namespace vendtechext.Helper
{
    public interface INotificationService
    {
        Task<NotificationDto> GetNotificationAsync(long id);
        List<NotificationDto> GetNotificationsAsync(string receiver);
        Task SaveNotificationAsync(string title, string description, string receiver, NotificationType type);
        Task UpdateNotificationReadStatusAsync(long id, string userId);
    }
    public interface INotificationChannel
    {
        void Send(NotificationRequest request);
    }

    public class SmsNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            string message = request.SmsMessage ?? request.Message;

            var requestmsg = new SMSRequest
            {
                Recipient = $"232{request.PhoneNo}",
                Payload = message
            };

            var json = JsonConvert.SerializeObject(requestmsg);

            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.BaseAddress = new Uri(DomainEnvironment.KwikTalkUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v2/submit");
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = client.SendAsync(httpRequest).Result;
                var stringResult = res.Content.ReadAsStringAsync().Result;
            }


        }
    }

    public class EmailNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            request.Message = request.EmailMessage ?? request.Message;

            using (var scope = ServiceLocator.Services!.CreateScope())
            {
                var services = scope.ServiceProvider;
                var _emailHelper = services.GetRequiredService<EmailHelper>();
                if (request.Emailtype == EmailTypeEnum.SendSimpleEmail)
                    new Emailer(_emailHelper).SendSimpleEmail(request);
                else if (request.Emailtype == EmailTypeEnum.SendEmailToIntegratorOnDepositApproval)
                    new Emailer(_emailHelper).SendEmailToIntegratorOnDepositApproval(request);
                else if (request.Emailtype == EmailTypeEnum.SendEmailToIntegratorOnBalanceLow)
                    new Emailer(_emailHelper).SendEmailToIntegratorOnBalanceLow(request);
                else if (request.Emailtype == EmailTypeEnum.SendEmailToIntegratorOnBalanceAlert)
                    new Emailer(_emailHelper).SendEmailToIntegratorOnBalanceAlert(request);
                else if (request.Emailtype == EmailTypeEnum.SendEmailToAdminOnPendingDeposits)
                    new Emailer(_emailHelper).SendEmailToAdminOnPendingDeposits(request);
            }

        }
    }

    public class PushNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            using (var scope = ServiceLocator.Services!.CreateScope())
            {
                var services = scope.ServiceProvider;
                var _integratorHubContext = services.GetRequiredService<IHubContext<CustomNotificationHub, ICustomNotificationHub>>();
                //var _pushService = services.GetRequiredService<IMobilePushService>();
                switch (request.NotificationEvent)
                {   
                    case NotificationEvent.SuccessNotificationCreated:
                        _integratorHubContext.Clients.Group(request.UserId).SuccessNotificationCreated(request?.PushMessage ?? "");
                        break;
                    case NotificationEvent.PendingDepositCreated:
                        _integratorHubContext.Clients.Group(request.UserId).PendingDepositCreated(request?.PushMessage ?? "");
                        break;
                    case NotificationEvent.PendingSalesCreated:
                        _integratorHubContext.Clients.Group(request.UserId).PendingSalesCreated(request?.PushMessage ?? "");
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public class DatabaseNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            using (var _context = new DataContext())
            {
                var notification = new Notification
                {
                    Title = request.Subject,
                    Description = request.Message,
                    Reciver = request.UserId,
                    Read = "",
                    Type = (int)request.NotificationType,
                    TargetId = request.TargetId,

                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }
        }
    }

    public class NotificationService
    {
        private readonly IEnumerable<INotificationChannel> _channels;

        public NotificationService(IEnumerable<INotificationChannel> channels)
        {
            _channels = channels;
        }

        public void SendNotification(NotificationRequest request)
        {
            if (request.SendSms)
                _channels.OfType<SmsNotificationChannel>().First().Send(request);

            if (request.SendEmail)
                _channels.OfType<EmailNotificationChannel>().First().Send(request);

            if (request.SendPush)
                _channels.OfType<PushNotificationChannel>().First().Send(request);

            if (request.SaveToDatabase)
                _channels.OfType<DatabaseNotificationChannel>().First().Send(request);
        }

        // 2. Gets notifications
        public NotificationDto GetNotification(long id)
        {
            using (var _context = new DataContext())
            {
                var notifications = _context.Notifications
               .Where(n => n.Id == id)
               .Select(n => new NotificationDto
               {
                   Id = n.Id,
                   Title = n.Title,
                   Description = n.Description,
                   Reciver = n.Reciver,
                   Read = true,
                   Type = n.Type,
                   Date = Utils.formatDate(n.CreatedAt),
                   TargetId = n.TargetId
               }).FirstOrDefault();

                return notifications;
            }
        }

        public List<NotificationDto> GetNotifications(string receiver)
        {
            using (var _context = new DataContext())
            {
                var notifications = _context.Notifications
                .Where(n => n.Reciver == receiver && n.Deleted == false)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Description = n.Description,
                    Reciver = n.Reciver,
                    Read = !string.IsNullOrEmpty(n.Read) && n.Read.Split(',', StringSplitOptions.None).Contains(receiver),
                    Type = n.Type,
                    Date = Utils.formatDate(n.CreatedAt),
                    TargetId = n.TargetId
                }).Take(20).ToList();

                return notifications;
            }
        }

        public long? GetNotificationId(string targetId)
        {
            using (var _context = new DataContext())
            {
                var id = _context.Notifications.Where(n => n.TargetId == targetId).FirstOrDefault()?.Id ?? null;
                return id;
            }
        }

        // 3. Updates notification read status
        public void UpdateNotificationReadStatus(long id, string userId)
        {
            using (var _context = new DataContext())
            {
                var notification = _context.Notifications.Find(id);
                //notification.Deleted = false;
                if (notification != null)
                {
                    // If 'Read' is empty or does not contain the userId, append the userId
                    if (string.IsNullOrEmpty(notification.Read) || !notification.Read.Split(',').Contains(userId))
                    {
                        // Append the userId to the Read field, using a comma as the separator
                        notification.Read = string.IsNullOrEmpty(notification.Read)
                            ? userId // If 'Read' is empty, just set it to the userId
                            : $"{notification.Read},{userId}"; // Otherwise, append the userId

                        _context.SaveChanges();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }

}
