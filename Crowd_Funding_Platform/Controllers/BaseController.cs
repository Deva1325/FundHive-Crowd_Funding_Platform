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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //int roleId = HttpContext.Session.GetInt32("UserRoleId") ?? 0;
            string isAdmin = HttpContext.Session.GetString("IsAdmin_ses");
            int roleId = 4;
            var tabs = _sidebar.GetTabsByRoleIdAsync(roleId,isAdmin).Result; // Sync for simplicity
            ViewBag.SidebarTabs = tabs;
            base.OnActionExecuting(context);
        }

    }
}
