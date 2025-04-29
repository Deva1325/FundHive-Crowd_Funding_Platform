using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(TblNotification notification);
        Task<IEnumerable<TblNotification>> GetNotificationsByUserAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
    }
}
