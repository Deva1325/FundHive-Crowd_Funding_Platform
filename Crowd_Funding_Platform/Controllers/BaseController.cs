using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Crowd_Funding_Platform.Controllers
{
    public class BaseController : Controller
    {
        private readonly ISidebarRepos _sidebar;

        public BaseController(ISidebarRepos sidebar)
        {
            _sidebar = sidebar;
        }

        public bool MainCheck()
        {
            int? userid = HttpContext.Session.GetInt32("UserId_ses");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("LoginCred")) && (userid == 0 || userid == null))
            {
                return false;
            }
            return true;
        }
        
       
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int? userid = HttpContext.Session.GetInt32("UserId_ses");

            //if (userid > 0)
            //{
            //    context.Result = new RedirectToActionResult("Login", "Account", null);
            //    return;
            //}

            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("LoginCred")) && (userid == 0 || userid == null))
            //{
            //    context.Result = new RedirectToActionResult("Login", "Account", null);
            //    return;
            //    return RedirectToAction("Login", "Account");
            //}

            int roleId = HttpContext.Session.GetInt32("UserRoleId") ?? 0;
            string isAdmin = HttpContext.Session.GetString("IsAdmin_ses");
            //int roleId = 4;
            var tabs = _sidebar.GetTabsByRoleIdAsync(roleId, isAdmin).Result; // Sync for simplicity
            ViewBag.SidebarTabs = tabs;
            base.OnActionExecuting(context);
        }

    }
}
