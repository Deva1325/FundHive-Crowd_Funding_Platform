using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.ViewComponents
{
    public class NotificationCountViewComponent : ViewComponent

    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationCountViewComponent(INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId") ?? 0;
            int count = userId != 0
                ? await _notificationService.GetUnreadCountAsync(userId)
                : 0;

            return View(count);
        }

    }
}
