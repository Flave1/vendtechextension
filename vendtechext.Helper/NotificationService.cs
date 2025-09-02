using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using System.Data;
using System.Threading.Channels;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{

    public interface INotificationChannel
    {
        void Send(NotificationRequest request);
    }

    public class SmsNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            string message = request.SmsMessage ?? request.Message;
            Console.WriteLine($"Sending SMS to {request.UserId}: {message}");
        }
    }

    public class EmailNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            string message = request.EmailMessage ?? request.Message;
            Console.WriteLine($"Sending Email to {request.UserId}: {message}");
        }
    }

    public class PushNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            string message = request.PushMessage ?? request.Message;
            Console.WriteLine($"Sending Push Notification to {request.UserId}: {message}");
        }
    }

    public class DatabaseNotificationChannel : INotificationChannel
    {
        public void Send(NotificationRequest request)
        {
            using(var _context = new DataContext())
            {
                var notification = new Notification
                {
                    Title = request.Title,
                    Description = request.Message,
                    Reciver = request.UserId,
                    Read = "",
                    CreatedAt = DateTime.UtcNow,
                    Type = request.Type,
                    TargetId = request.TargetId
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

        // 1. Saves a notification
        public void SaveNotification(string title, string description, string receiver, NotificationType type, string target)
        {
            using(var _context = new DataContext())
            {
                var notification = new Notification
                {
                    Title = title,
                    Description = description,
                    Reciver = receiver,
                    Read = "",
                    CreatedAt = DateTime.UtcNow,
                    Type = (int)type,
                    TargetId = target
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }
        }

        // 2. Gets notifications
        public NotificationDto GetNotification(long id)
        {
           using(var _context = new DataContext())
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
            using(var _context = new DataContext())
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
            using(var _context = new DataContext())
            {
                var id = _context.Notifications.Where(n => n.TargetId == targetId).FirstOrDefault()?.Id ?? null;
                return id;
            }
        }

        // 3. Updates notification read status
        public void UpdateNotificationReadStatus(long id, string userId)
        {
           using(var _context = new DataContext())
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
