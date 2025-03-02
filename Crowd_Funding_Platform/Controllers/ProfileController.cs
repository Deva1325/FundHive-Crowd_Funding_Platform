using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IUserProfile;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Cms;

namespace Crowd_Funding_Platform.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IProfileRepos _profileRepos;
        private readonly DbMain_CFS _CFS;

        public ProfileController(IProfileRepos profileRepos,DbMain_CFS dbMain_CFS,ISidebarRepos sidebar ) : base(sidebar)
        {
            _profileRepos = profileRepos;
            _CFS = dbMain_CFS;
        }

    

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            if(userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _profileRepos.GetAllUsersData(userId.Value);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile() {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> EditProfile() {
        //    return View();
        //}
    }
}
