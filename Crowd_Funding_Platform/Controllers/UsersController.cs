using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using X.PagedList.Extensions;


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
        public async Task<IActionResult> ContributorsList(string searchString, string category, int? page)
        {
            var contributors = await _user.GetAllContributorsAsync();

            // Get unique categories
            var categories = contributors
                             .Where(c => c.Campaign?.Category != null && !string.IsNullOrEmpty(c.Campaign.Category.Name))
                             .Select(c => c.Campaign.Category.Name)
                             .Distinct()
                             .ToList();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                contributors = contributors
                    .Where(c => c.Contributor?.Username != null &&
                                c.Contributor.Username.ToLower().Contains(searchString.ToLower()))
                    .ToList();
            }

            // Category filter
            if (!string.IsNullOrEmpty(category))
            {
                contributors = contributors
                    .Where(c => c.Campaign?.Category?.Name == category)
                    .ToList();
            }

            // Default page = 1, pageSize = 10
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var pagedContributors = contributors.ToPagedList(pageNumber, pageSize);

            ViewBag.CurrentFilter = searchString;
            ViewBag.SelectedCategory = category;
            ViewBag.Categories = categories;

            return View(pagedContributors);
        }

        //[HttpGet]
        //public async Task<IActionResult> ContributorsList()
        //{
        //    List<Contribution> contributions= await _user.GetAllContributorsAsync();
        //    return View(contributions);
        //}

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

        //[HttpGet]
        //public async Task<IActionResult> MyContributions(string searchString, string category, int? page)
        //{
        //    var contributors = await _user.GetAllContributorsAsync();

        //    // Get unique categories
        //    var categories = contributors
        //                     .Where(c => c.Campaign?.Category != null && !string.IsNullOrEmpty(c.Campaign.Category.Name))
        //                     .Select(c => c.Campaign.Category.Name)
        //                     .Distinct()
        //                     .ToList();

        //    // Search filter
        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        contributors = contributors
        //            .Where(c => c.Contributor?.Username != null &&
        //                        c.Contributor.Username.ToLower().Contains(searchString.ToLower()))
        //            .ToList();
        //    }

        //    // Category filter
        //    if (!string.IsNullOrEmpty(category))
        //    {
        //        contributors = contributors
        //            .Where(c => c.Campaign?.Category?.Name == category)
        //            .ToList();
        //    }

        //    // Default page = 1, pageSize = 10
        //    int pageSize = 5;
        //    int pageNumber = page ?? 1;

        //    var pagedContributors = contributors.ToPagedList(pageNumber, pageSize);

        //    ViewBag.CurrentFilter = searchString;
        //    ViewBag.SelectedCategory = category;
        //    ViewBag.Categories = categories;

        //    return View(pagedContributors);
        //}

    }
}
