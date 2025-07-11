﻿using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepos;
        private readonly IAccountRepos _accountRepos;
        private readonly DbMain_CFS _CFS;
        public NotificationController(DbMain_CFS dbMain_CFS, INotificationService notificationService,INotificationRepository notificationRepository,IAccountRepos accountRepos)
        {
             _CFS = dbMain_CFS;
            _notificationService = notificationService;
            _notificationRepos = notificationRepository;
            _accountRepos= accountRepos;
        }

        //current method 
        //public async Task<IActionResult> Index()
        //{
        //    int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

        //    if (userId != 0)
        //    {
        //        // Get unread notification count
        //        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
        //        ViewBag.NotificationCount = unreadCount;
        //    }
        //    else
        //    {
        //        ViewBag.NotificationCount = 0; // Default value if no userId is found
        //    }
        //    var notifications = await _notificationService.GetUserNotificationsAsync(userId);

        //    return View(notifications);
        //}

        public async Task<IActionResult> Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Return unauthorized if no session
            if (userId == 0)
                return RedirectToAction("unAuthorized401", "Error");

            // Get unread count
            ViewBag.NotificationCount = await _notificationService.GetUnreadCountAsync(userId);

            // Admin ID is hardcoded as 2041
            bool isAdmin = userId == 2041;

            // Admin sees only admin-related notifications
            // Other users see only their notifications
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            return View(notifications);
        }

        // ✅ Mark Notification as Read
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId);
            return RedirectToAction("Index"); // Refresh notification list
        }


        }
}
