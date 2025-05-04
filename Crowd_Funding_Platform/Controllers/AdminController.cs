using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Controllers
{
    public class AdminController : BaseController
    {

        private readonly IActivityRepository _activityRepository;
        private readonly DbMain_CFS _context;
        //private const string AdminPassword = "admin@123"; // 🔒 Change this to your actual admin password or store securely

        public AdminController(IActivityRepository activityRepository, DbMain_CFS context,ISidebarRepos sidebar) : base(sidebar)
        {
            _activityRepository = activityRepository;
            _context = context;
            
        }

        //Current working method 
        //[Route("AuditLogs")]
        //[HttpGet]
        //public IActionResult AuditLogs(string? username, string? activityType, string? tableName, DateTime? fromDate, DateTime? toDate)
        //{
        //    var isAdmin = HttpContext.Session.GetString("IsAdmin_ses");
        //    var isCreator = HttpContext.Session.GetString("IsCreatorApproved");
        //    var userId = HttpContext.Session.GetInt32("UserId_ses");

        //    ViewBag.username = username;

        //    if (isAdmin == "true")
        //    {
        //        // Existing admin logic
        //        var logs = _activityRepository.GetAllAuditLogs()
        //                            .Where(l => !l.IsDeleted).ToList();

        //        if (!string.IsNullOrEmpty(username))
        //            logs = logs.Where(l => l.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();

        //        if (!string.IsNullOrEmpty(activityType))
        //            logs = logs.Where(l => l.ActivityType.Contains(activityType, StringComparison.OrdinalIgnoreCase)).ToList();

        //        if (!string.IsNullOrEmpty(tableName))
        //            logs = logs.Where(l => !string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(tableName, StringComparison.OrdinalIgnoreCase)).ToList();

        //        if (fromDate.HasValue)
        //            logs = logs.Where(l => l.Timestamp >= fromDate.Value).ToList();

        //        if (toDate.HasValue)
        //            logs = logs.Where(l => l.Timestamp <= toDate.Value).ToList();

        //        return View("AuditLogs", logs); // 👈 Use Admin-specific view
        //    }

        //    else if (isCreator == "true" && userId.HasValue)
        //    {
        //        // Creator-specific logs
        //        var creatorCampaignIds = _context.Campaigns
        //                                         .Where(c => c.CreatorId == userId.Value)
        //                                         .Select(c => c.CampaignId)
        //                                         .ToList();

        //        // Example: Users who donated to this creator's campaigns
        //        var relatedUserIds = _context.Contributions
        //                                     .Where(d => creatorCampaignIds.Contains(d.CampaignId))
        //                                     .Select(d => d.ContributorId)
        //                                     .Distinct()    
        //                                     .ToList();

        //        var creatorLogs = _context.TblAuditLogs
        //                                  .Where(log => !log.IsDeleted &&  relatedUserIds.Contains(log.UserId))
        //                                  .ToList();

        //        return View("CreatorAuditLogs", creatorLogs); // 👈 Use Creator-specific view
        //    }

        //    TempData["Error"] = "You are not authorized to view audit logs.";
        //    return RedirectToAction("Index", "Home");
        //}


        [Route("AuditLogs")]
        [HttpGet]
        public IActionResult AuditLogs(string? username, string? activityType, string? tableName, DateTime? fromDate, DateTime? toDate, string? viewAs)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin_ses");
            var isCreator = HttpContext.Session.GetString("IsCreatorApproved");
            var userId = HttpContext.Session.GetInt32("UserId_ses");

            viewAs ??= "admin"; // default to "admin" if null
            ViewBag.ViewAs = viewAs;

            if (viewAs == "admin" && isAdmin == "true")
            {
                var logs = _activityRepository.GetAllAuditLogs()
                                .Where(l => !l.IsDeleted).ToList();

                if (!string.IsNullOrEmpty(username))
                    logs = logs.Where(l => l.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrEmpty(activityType))
                    logs = logs.Where(l => l.ActivityType.Contains(activityType, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrEmpty(tableName))
                    logs = logs.Where(l => !string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(tableName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (fromDate.HasValue)
                    logs = logs.Where(l => l.Timestamp >= fromDate.Value).ToList();

                if (toDate.HasValue)
                    logs = logs.Where(l => l.Timestamp <= toDate.Value).ToList();

                return View("AuditLogs", logs);
            }
            else if (viewAs == "creator" && isCreator == "true" && userId.HasValue)
            {
                var creatorCampaignIds = _context.Campaigns
                                        .Where(c => c.CreatorId == userId.Value)
                                        .Select(c => c.CampaignId)
                                        .ToList();

                var relatedUserIds = _context.Contributions
                                    .Where(d => creatorCampaignIds.Contains(d.CampaignId))
                                    .Select(d => d.ContributorId)
                                    .Distinct()
                                    .ToList();

                var creatorLogs = _context.TblAuditLogs
                                    .Where(log => !log.IsDeleted && relatedUserIds.Contains(log.UserId))
                                    .ToList();

                return View("CreatorAuditLogs", creatorLogs);
            }
            else
            {
                // fallback
                TempData["Error"] = "You are not authorized to view audit logs.";
                return RedirectToAction("Index", "Home");
            }

           
        }



        //[Route("AuditLogs")]
        //[HttpGet]
        //public IActionResult AuditLogs(string? username, string? activityType, string? tableName, DateTime? fromDate, DateTime? toDate)
        //{
        //    var logs = _activityRepository.GetAllAuditLogs()
        //                        .Where(l => !l.IsDeleted).ToList();

        //    //var logs = _activityRepository.GetAllAuditLogs();

        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        logs = logs.Where(l => l.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();
        //    }

        //    if (!string.IsNullOrEmpty(activityType))
        //    {
        //        logs = logs.Where(l => l.ActivityType.Contains(activityType, StringComparison.OrdinalIgnoreCase)).ToList();
        //    }

        //    if (!string.IsNullOrEmpty(tableName))
        //    {
        //        logs = logs.Where(l => !string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(tableName, StringComparison.OrdinalIgnoreCase)).ToList();
        //    }

        //    if (fromDate.HasValue)
        //    {
        //        logs = logs.Where(l => l.Timestamp >= fromDate.Value).ToList();
        //    }

        //    if (toDate.HasValue)
        //    {
        //        logs = logs.Where(l => l.Timestamp <= toDate.Value).ToList();
        //    }

        //    return View(logs);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearLogs(string adminPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var currentUser = HttpContext.Session.GetString("UserName");

            if (!userId.HasValue || string.IsNullOrEmpty(currentUser))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("AuditLogs");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value && u.Username == currentUser);

            if (user == null || !BCrypt.Net.BCrypt.Verify(adminPassword, user.PasswordHash))
            {
                TempData["Error"] = "Incorrect admin password. Logs not cleared.";
                return RedirectToAction("AuditLogs");
            }

            // Soft delete all logs
            var allLogs = _context.TblAuditLogs.Where(l => !l.IsDeleted).ToList();
            foreach (var log in allLogs)
            {
                log.IsDeleted = true;
            }

            _context.SaveChanges();

            //// Log the action
            //_activityRepository.AddNewActivity(user.UserId, "Clear Logs", "Cleared all audit logs");

            TempData["Success"] = "All audit logs have been cleared successfully.";
            return RedirectToAction("AuditLogs");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCreatorLogs(string creatorPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId_ses");
            var currentUser = HttpContext.Session.GetString("UserName");

            if (!userId.HasValue || string.IsNullOrEmpty(currentUser))
            {
                TempData["Error"] = "User session expired. Please log in again.";
                return RedirectToAction("AuditLogs");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value && u.Username == currentUser);

            if (user == null || !BCrypt.Net.BCrypt.Verify(creatorPassword, user.PasswordHash))
            {
                TempData["Error"] = "Incorrect password. Logs not cleared.";
                return RedirectToAction("AuditLogs");
            }

            // Get all campaign IDs of this creator
            var creatorCampaignIds = _context.Campaigns
                                             .Where(c => c.CreatorId == userId.Value)
                                             .Select(c => c.CampaignId)
                                             .ToList();

            // Fetch and soft-delete only logs related to the creator's campaigns
            //var creatorLogs = _context.TblAuditLogs
            //                          .Where(log => !log.IsDeleted &&
            //                                        log.RelatedCampaignId != null &&
            //                                        creatorCampaignIds.Contains(log.RelatedCampaignId.Value))
            //                          .ToList();

            // Step 1: Get all contributor IDs for the creator's campaigns
            var contributorIds = _context.Contributions
                                         .Where(c => creatorCampaignIds.Contains(c.CampaignId))
                                         .Select(c => c.ContributorId)
                                         .Distinct()
                                         .ToList();

            // Step 2: Soft delete only those logs
            var creatorLogs = _context.TblAuditLogs
                                      .Where(log => !log.IsDeleted && contributorIds.Contains(log.UserId))
                                      .ToList();

            //var creatorLogs = _context.TblAuditLogs
            //                          .Where(log => !log.IsDeleted)
            //                          .ToList();
            foreach (var log in creatorLogs)
            {
                log.IsDeleted = true;
            }

            _context.SaveChanges();

            //// Log this action
            //_activityRepository.AddNewActivity(user.UserId, "Clear Logs", "Cleared logs related to creator's campaigns");

            TempData["Success"] = "Your campaign logs have been cleared successfully.";
            return RedirectToAction("AuditLogs");
        }



    }
}


//[HttpPost]
//[ValidateAntiForgeryToken]
//public IActionResult ClearLogs(string adminPassword)
//{
//    var currentUser = HttpContext.Session.GetString("UserName"); // assuming session contains username
//    var admin = _context.Users.FirstOrDefault(u => u.Username == currentUser && u.PasswordHash == adminPassword);

//    if (admin == null)
//    {
//        TempData["Error"] = "Incorrect admin password. Logs not cleared.";
//        return RedirectToAction("AuditLogs");
//    }

//    var allLogs = _context.TblAuditLogs.Where(l => !l.IsDeleted).ToList();

//    foreach (var log in allLogs)
//    {
//        log.IsDeleted = true; // soft delete
//    }

//    _context.SaveChanges();

//    // Meta-log the clear action
//    _activityRepository.AddNewActivity(admin.UserId, "Clear Logs", "Cleared all audit logs");

//    TempData["Success"] = "All audit logs have been cleared successfully.";
//    return RedirectToAction("AuditLogs");
//}


//public IActionResult AuditLogs()
//{
//    var logs = _activityRepository.GetAllAuditLogs();
//    return View(logs);
//}

//public IActionResult Index()
//{
//    return View();
//}


