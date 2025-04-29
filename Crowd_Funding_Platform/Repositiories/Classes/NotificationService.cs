using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class NotificationService : INotificationService
    {
        private readonly DbMain_CFS _CFS;
        private readonly INotificationRepository _notificationRepos;
        
        public NotificationService(DbMain_CFS dbMain_CFS,INotificationRepository notificationRepository)
        {
            _CFS = dbMain_CFS;
            _notificationRepos = notificationRepository;
        }

        public async Task SendNotificationAsync(int userId, string message, int type, int? relatedId, string moduleType)
        {
            var notification = new TblNotification
            {
                UserId = userId,
                Message = message,
                Type = type,
                RelatedId = relatedId,
                ModuleType = moduleType,
                Date = DateTime.Now,
                IsRead = false
            };

            await _notificationRepos.AddNotificationAsync(notification);
        }

        public async Task<IEnumerable<TblNotification>> GetUserNotificationsAsync(int userId)
        {
            return await _notificationRepos.GetNotificationsByUserAsync(userId);
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            await _notificationRepos.MarkAsReadAsync(notificationId);
        }
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _CFS.TblNotifications
                                 .Where(n => n.UserId == userId && n.IsRead == false)
                                 .CountAsync();
        }
        public async Task SendReminderNotificationAsync(int userId, string message)
        {
            await SendNotificationAsync(userId, message, (int)NotificationType.Reminder, null, "acceptreject");
        }
    }
    public enum NotificationType
    {
        Reminder = 1
    }
}
