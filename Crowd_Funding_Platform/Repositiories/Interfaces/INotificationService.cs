using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string message, int type, int? relatedId, string moduleType);
        Task<IEnumerable<TblNotification>> GetUserNotificationsAsync(int userId);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);


        // Method to send reminder notifications
        Task SendReminderNotificationAsync(int userId, string message);

        //// Method to send processed notifications (for auto-deduct or completed transactions)
        //Task SendProcessedNotificationAsync(int userId, string message);
    }
}
