using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IUser _user;
        private readonly DbMain_CFS _CFS;

        public UsersController(IUser user,DbMain_CFS dbMain_CFS, ISidebarRepos sidebar) : base(sidebar)
        {
            _CFS = dbMain_CFS;
            _user = user;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreatorsList() 
        {    
            List<CreatorApplication> creators = await _user.GetAllCreatorsAsync();
            return View(creators);
        }

        [HttpGet]
        public IActionResult ContributorsList()
        {
            return View();
        }
    }
}
