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
            return View();
        }
    }
}
