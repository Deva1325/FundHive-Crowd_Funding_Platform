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

        [HttpGet, ActionName("DeleteCreator")]
        public async Task<IActionResult> Delete(int id)
        {
            var deltCam = await _user.GetCreatorsById(id);
            return View(deltCam);
        }

        [HttpPost, ActionName("DeleteCreator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var DelCam = await _user.DeleteCreator(id);

                return RedirectToAction("CreatorsList", "Users");
                //return View(DelCam);
            }
            catch (Exception ex)
            {
                throw new Exception("Delete failed", ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreatorsDetails(int id)
        {
            var campaign = await _user.GetCreatorsById(id);
            return View(campaign);
        }


    }
}
