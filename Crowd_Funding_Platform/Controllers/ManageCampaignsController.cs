using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Crowd_Funding_Platform.Controllers
{
    public class ManageCampaignsController : BaseController
    {
        private readonly ICampaignsRepos _campaign;
        private readonly DbMain_CFS _CFS;

        public ManageCampaignsController(ICampaignsRepos campaign,DbMain_CFS dbMain_CFS , ISidebarRepos sidebar) : base(sidebar)
        {
            _campaign = campaign;
            _CFS= dbMain_CFS;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ShowCampaignCases()
        {
            return View();
        }

        public async Task<IActionResult> DetailCampaignCases()
        {
            return View();
        }

        public async Task<IActionResult> PendingCampaigns()
        {
            List<CreatorApplication> pendingCampaigns = await _campaign.GetPendingCampaigns();
            return View(pendingCampaigns);
           // return View();

        }

        [HttpGet]
        public async Task<IActionResult> SaveCampaigns(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId_ses");
            var isCreatorApproved = (HttpContext.Session.GetString("IsCreatorApproved") ?? "false") == "true";

            Campaign campaign = new Campaign();
            if ( id > 0)
            {
                campaign = await _CFS.Campaigns.FirstOrDefaultAsync(c => c.CampaignId == id);
            }

            ViewBag.Categories = new SelectList(await _CFS.Categories.ToListAsync(), "CategoryId", "Name");

            //// Fetch roles(Users) dynamically excluding Admin (RoleId = 4)
            //ViewBag.Categories = new SelectList(await _CFS.Categories
            //    .Select(c => new { c.CategoryId, c.Name })
            //    .ToListAsync(), "CategoryId", "Name");                                           

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!isCreatorApproved)
            {
                return RedirectToAction("SaveCampaigns", "ManageCampaigns");
            }

            return View(campaign);
        }

      
        [HttpPost]
        public async Task<IActionResult> SaveCampaigns(Campaign campaign, IFormFile? MediaUrl)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId_ses");

                if (userId == null)
                {
                    return Json(new { success = false, message = "Session expired. Please login again." });
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.Today);

                if (campaign.StartDate > today)
                {
                    campaign.Status = "Upcoming";
                }
                else if (campaign.StartDate <= today && campaign.EndDate >= today)
                {
                    campaign.Status = "Ongoing";
                }
                else if (campaign.EndDate < today)
                {
                    campaign.Status = "Completed";
                }

                var result = await _campaign.SaveCampaigns(campaign, userId.Value,MediaUrl);

                if (!result.success)
                {
                    return Json(new { success = false, message = result.message });
                }

                return Json(new
                {
                    success = true,
                    message = "Campaign saved successfully!"              
                });
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CampaignsList()
        {
            try
            {
                var campaigns = await _campaign.GetAllCampaigns();
                return View(campaigns);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewCreatorDocument(int id)
        {
            //var creatorDetails = _campaign.GetCreatorApplicationDetails(id);
            //if (creatorDetails == null) return NotFound();

            //return View(creatorDetails);
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ApproveCreator(int id)
        {
            var result = await _campaign.ApproveCreator(id);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> RejectCreator(int id)
        {
            var result = await _campaign.RejectCreator(id);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var deltCam = await _campaign.GetCampaignById(id);
            return View(deltCam);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var DelCam = await _campaign.DeleteCampaign(id);

                return RedirectToAction("CampaignsList", "ManageCampaigns");
                //return View(DelCam);
            }
            catch (Exception ex)
            {
                throw new Exception("Delete failed", ex);
            }
        }

        [HttpGet, ActionName("PendingCampaignsDetails")]
        public async Task<IActionResult> Details(int id)
        {
            var campaign = await _campaign.GetApplicationById(id);
            return View(campaign);
        }


        [HttpGet]
        public async Task<IActionResult> CreatorList()
        {
            return View();
            //try
            //{
            //    var campaigns = await _campaign.GetAllCampaigns();
            //    return View(campaigns);
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            //}
        }

    }
}