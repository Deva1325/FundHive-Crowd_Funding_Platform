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

        [Route("AuditLogs")]
        [HttpGet]
        public IActionResult AuditLogs(string? username, string? activityType, string? tableName, DateTime? fromDate, DateTime? toDate)
        {
            var logs = _activityRepository.GetAllAuditLogs()
                                .Where(l => !l.IsDeleted).ToList();

            //var logs = _activityRepository.GetAllAuditLogs();

            if (!string.IsNullOrEmpty(username))
            {
                logs = logs.Where(l => l.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(activityType))
            {
                logs = logs.Where(l => l.ActivityType.Contains(activityType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                logs = logs.Where(l => !string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(tableName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (fromDate.HasValue)
            {
                logs = logs.Where(l => l.Timestamp >= fromDate.Value).ToList();
            }

            if (toDate.HasValue)
            {
                logs = logs.Where(l => l.Timestamp <= toDate.Value).ToList();
            }

            return View(logs);
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

            // Log the action
            _activityRepository.AddNewActivity(user.UserId, "Clear Logs", "Cleared all audit logs");

            TempData["Success"] = "All audit logs have been cleared successfully.";
            return RedirectToAction("AuditLogs");
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ClearLogs(string adminPassword)
        //{
        //    string originalPassword = "";
        //    var userid = HttpContext.Session.GetInt32("UserId");
        //    var currentUser = HttpContext.Session.GetString("UserName");
        //    if (userid.HasValue)
        //    {
        //        var user = _context.Users.FirstOrDefault(u => u.UserId == userid);
                 
        //        originalPassword = user.PasswordHash;

        //        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

        //    }

        //    var admin = _context.Users.FirstOrDefault(u => u.UserId == userid && u.PasswordHash == originalPassword);

        //    if (admin == null)
        //    {
        //        TempData["Error"] = "Incorrect admin password. Logs not cleared.";
        //        return RedirectToAction("AuditLogs");
        //    }

        //    var allLogs = _context.TblAuditLogs.Where(l => !l.IsDeleted).ToList();

        //    foreach (var log in allLogs)
        //    {
        //        log.IsDeleted = true;
        //    }

        //    _context.SaveChanges();

        //    _activityRepository.AddNewActivity(admin.UserId, "Clear Logs", "Cleared all audit logs");

        //    TempData["Success"] = "All audit logs have been cleared successfully.";
        //    return RedirectToAction("AuditLogs");
        //}


    }
}


//public IActionResult AuditLogs()
//{
//    var logs = _activityRepository.GetAllAuditLogs();
//    return View(logs);
//}

//public IActionResult Index()
//{
//    return View();
//}


