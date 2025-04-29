using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class ErrorController : Controller
    {
        
        public IActionResult unAuthorized401()
        {
            return View();
        }
    }
}
