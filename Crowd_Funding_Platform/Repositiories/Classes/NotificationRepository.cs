using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DbMain_CFS _CFS;

        public NotificationRepository(DbMain_CFS dbMain_CFS)
        {
             _CFS=dbMain_CFS;
        }

        public async Task AddNotificationAsync(TblNotification notification)
        {
            await _CFS.TblNotifications.AddAsync(notification);
            await _CFS.SaveChangesAsync();
        }

        public async Task<IEnumerable<TblNotification>> GetNotificationsByUserAsync(int userId)
        {
            return await _CFS.TblNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.Date)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _CFS.TblNotifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _CFS.SaveChangesAsync();
            }
        }
    }
}
