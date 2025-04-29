using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(ISidebarRepos sidebar) : base(sidebar)
        {
            
        }


        [ActionName("Dashboard")]
        public IActionResult Index()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin_ses");
            var isCreatorApproved = HttpContext.Session.GetString("IsCreatorApproved");

            if (isAdmin == "true")
            {
                return RedirectToAction("Index", "AdminDashboard");
            }
            else if (isCreatorApproved == "true")
            {
                return RedirectToAction("Index", "CreatorDashboard");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            //if (!MainCheck())
            //    return RedirectToAction("Login","Account");
            //return View();
        }
    }
}
