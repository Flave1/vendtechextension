using vendtechext.Contracts;
using vendtechext.DAL.Common;
using vendtechext.DAL.Models;

namespace vendtechext.BLL.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> GetNotificationAsync(long id);
        List<NotificationDto> GetNotificationsAsync(string receiver);
        Task SaveNotificationAsync(string title, string description, string receiver, NotificationType type);
        Task UpdateNotificationReadStatusAsync(long id, string userId);
    }
}
