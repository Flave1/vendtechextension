using Microsoft.EntityFrameworkCore;
using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Services
{
    public class NotificationHelper
    {
        private readonly DataContext _context; // Your DbContext for accessing the database

        public NotificationHelper(DataContext context)
        {
            _context = context;
        }

        // 1. Saves a notification
        public void SaveNotification(string title, string description, string receiver, NotificationType type, string target)
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

        // 2. Gets notifications
        public NotificationDto GetNotification(long id)
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

        public List<NotificationDto> GetNotifications(string receiver)
        {
            var notifications = _context.Notifications
                .Where(n => n.Reciver == receiver)
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
                }).ToList();

            return notifications;
        }

        // 3. Updates notification read status
        public void UpdateNotificationReadStatus(long id, string userId)
        {
            var notification = _context.Notifications.Find(id);

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
