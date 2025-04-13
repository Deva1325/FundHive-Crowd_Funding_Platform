using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList.Extensions;

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

        [HttpGet]
        public async Task<IActionResult> ShowCampaignCases()
        {
            var campaigns = _campaign.ShowCampaignCases();
            return View(campaigns);
        }

        [HttpGet]
        public async Task<IActionResult> DetailCampaignCases(int id)
        {
            var campaign = _campaign.DetailCampaignCases(id);

            if (campaign == null)
            {
                return NotFound();
            }

            return View(campaign);
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
                return RedirectToAction("ApplyForCreator", "Campaign");
            }

            return View(campaign);
        }


        [HttpPost]
        public async Task<IActionResult> SaveCampaigns(Campaign campaign, IFormFile? ImageFile, List<IFormFile>? GalleryImages)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId_ses");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Session expired. Please login again." });
                }

                DateOnly today = DateOnly.FromDateTime(DateTime.Today);

                // ✅ Prevent editing campaigns that are not 'Upcoming'
                if (campaign.CampaignId != 0)
                {
                    var existing = await _campaign.GetCampaignById(campaign.CampaignId);
                    if (existing != null && existing.Status != "Upcoming")
                    {
                        return Json(new { success = false, message = "Only 'Upcoming' campaigns can be edited." });
                    }
                }
                

                // ✅ Validate only if dates are modified
                if (campaign.StartDate < today)
                {
                    return Json(new { success = false, message = "Start date cannot be before today." });
                }

                if (campaign.EndDate < today)
                {
                    return Json(new { success = false, message = "End date cannot be before today." });
                }

                if (campaign.EndDate < campaign.StartDate)
                {
                    return Json(new { success = false, message = "End date cannot be before start date." });
                }


                // Set campaign status based on dates
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

                // Pass the file parameter (now named ImageFile) to the repository method
                var result = await _campaign.SaveCampaigns(campaign, userId.Value, ImageFile,GalleryImages);

                if (!result.success)
                {
                    return Json(new { success = false, message = result.message });
                }

                return Json(new { success = true, message = "Campaign saved successfully!" });
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }


        // Old Code

        //[HttpPost]
        //public async Task<IActionResult> SaveCampaigns(Campaign campaign, IFormFile? ImageFile)
        //{
        //    try
        //    {
        //        var userId = HttpContext.Session.GetInt32("UserId_ses");
        //        if (userId == null)
        //        {
        //            return Json(new { success = false, message = "Session expired. Please login again." });
        //        }

        //        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        //        // ✅ Prevent editing campaigns that are not 'Upcoming'
        //        if (campaign.CampaignId != 0)
        //        {
        //            var existing = await _campaign.GetCampaignById(campaign.CampaignId);
        //            if (existing != null && existing.Status != "Upcoming")
        //            {
        //                return Json(new { success = false, message = "Only 'Upcoming' campaigns can be edited." });
        //            }
        //        }
        //        //var existingCampaign = await _CFS.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.CampaignId == campaign.CampaignId);

        //        //if (existingCampaign != null)
        //        //{
        //        //    // ✅ Skip validation if dates are unchanged
        //        //    if (campaign.StartDate == existingCampaign.StartDate && campaign.EndDate == existingCampaign.EndDate)
        //        //    {
        //        //        var res = await _campaign.SaveCampaigns(campaign, userId.Value, ImageFile);
        //        //        return Json(new { success = res.success, message = res.message });
        //        //    }
        //        //}

        //        // ✅ Validate only if dates are modified
        //        if (campaign.StartDate < today)
        //        {
        //            return Json(new { success = false, message = "Start date cannot be before today." });
        //        }

        //        if (campaign.EndDate < today)
        //        {
        //            return Json(new { success = false, message = "End date cannot be before today." });
        //        }

        //        if (campaign.EndDate < campaign.StartDate)
        //        {
        //            return Json(new { success = false, message = "End date cannot be before start date." });
        //        }


        //        // Set campaign status based on dates
        //        if (campaign.StartDate > today)
        //        {
        //            campaign.Status = "Upcoming";
        //        }
        //        else if (campaign.StartDate <= today && campaign.EndDate >= today)
        //        {
        //            campaign.Status = "Ongoing";
        //        }
        //        else if (campaign.EndDate < today)
        //        {
        //            campaign.Status = "Completed";
        //        }

        //        // Pass the file parameter (now named ImageFile) to the repository method
        //        var result = await _campaign.SaveCampaigns(campaign, userId.Value, ImageFile);

        //        if (!result.success)
        //        {
        //            return Json(new { success = false, message = result.message });
        //        }

        //        return Json(new { success = true, message = "Campaign saved successfully!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.Message);
        //        return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
        //    }
        //}


        [HttpGet, ActionName("CampaignsList")]
        public async Task<IActionResult> CampaignsList(string searchTerm, string categoryFilter, DateOnly? startDate, DateOnly? endDate, int? page)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId_ses");
                string isAdmin = HttpContext.Session.GetString("IsAdmin_ses");

                if (userId == null && isAdmin != "true")
                    return RedirectToAction("Login", "Account");

                List<Campaign> campaigns;

                if (isAdmin == "true")
                    campaigns = await _campaign.GetAllCampaigns();
                else
                    campaigns = await _campaign.GetCampaignsByCreator(userId.Value);

                DateOnly today = DateOnly.FromDateTime(DateTime.Today);

                foreach (var campaign in campaigns)
                {
                    // Set status based on StartDate and EndDate
                    if (campaign.StartDate > today)
                        campaign.Status = "Upcoming";
                    else if (campaign.StartDate <= today && campaign.EndDate >= today)
                        campaign.Status = "Ongoing";
                    else if (campaign.EndDate < today)
                        campaign.Status = "Completed";

                    // Add total contributors
                    campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId);
                    //campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId) ?? 0;

                }

                // Filter: Search by title
                //if (!string.IsNullOrEmpty(searchTerm))
                //{
                //    campaigns = campaigns.Where(c => c.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                //}


                // Search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    campaigns = campaigns
                        .Where(c => c.Title != null &&
                                    c.Title.ToLower().Contains(searchTerm.ToLower()))
                        .ToList();
                }


                // Filter: Date Range
                if (startDate.HasValue)
                {
                    campaigns = campaigns.Where(c => c.StartDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    campaigns = campaigns.Where(c => c.EndDate <= endDate.Value).ToList();
                }

                // Fetch all categories with ID and Name (for dropdown)
                var allCategories = await _CFS.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                ViewBag.Categories = allCategories;

                // Category filter using ID (match with stored CategoryId in Campaign)
                if (!string.IsNullOrEmpty(categoryFilter) && int.TryParse(categoryFilter, out int categoryId))
                {
                    campaigns = campaigns
                        .Where(c => c.CategoryId == categoryId)
                        .ToList();
                }

                // Pass selected category back to view
                ViewBag.SelectedCategoryId = categoryFilter;

                // Pagination
                int pageSize = 5;
                int pageNumber = page ?? 1;
                var pagedCampaigns = campaigns.ToPagedList(pageNumber, pageSize);

                ViewBag.SearchTerm = searchTerm;
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

                return View(pagedCampaigns);

                //return View(campaigns);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> CampaignsList(string searchTerm, string statusFilter, string categoryFilter, DateOnly? startDate, DateOnly? endDate, int? page)
        //{
        //    try
        //    {
        //        int? userId = HttpContext.Session.GetInt32("UserId_ses");
        //        string isAdmin = HttpContext.Session.GetString("IsAdmin_ses");

        //        List<Campaign> campaigns;

        //        if (isAdmin == "true")
        //            campaigns = await _campaign.GetAllCampaigns();
        //        else
        //            campaigns = await _campaign.GetCampaignsByCreator(userId.Value);

        //        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        //        foreach (var campaign in campaigns)
        //        {
        //            // Set status based on StartDate and EndDate
        //            if (campaign.StartDate > today)
        //                campaign.Status = "Upcoming";
        //            else if (campaign.StartDate <= today && campaign.EndDate >= today)
        //                campaign.Status = "Ongoing";
        //            else if (campaign.EndDate < today)
        //                campaign.Status = "Completed";

        //            // Add total contributors
        //            campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId);
        //            //campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId) ?? 0;

        //        }

        //        // Filter: Search by title
        //        if (!string.IsNullOrEmpty(searchTerm))
        //        {
        //            campaigns = campaigns.Where(c => c.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        //        }

        //        // Filter: Status
        //        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
        //        {
        //            campaigns = campaigns.Where(c => c.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        //        }

        //        // Filter: Date Range
        //        if (startDate.HasValue)
        //        {
        //            campaigns = campaigns.Where(c => c.StartDate >= startDate.Value).ToList();
        //        }

        //        if (endDate.HasValue)
        //        {
        //            campaigns = campaigns.Where(c => c.EndDate <= endDate.Value).ToList();
        //        }

        //        // Fetch all categories with ID and Name (for dropdown)
        //        var allCategories = await _CFS.Categories
        //            .Select(c => new SelectListItem
        //            {
        //                Value = c.CategoryId.ToString(),
        //                Text = c.Name
        //            })
        //            .ToListAsync();

        //        ViewBag.Categories = allCategories;

        //        // Category filter using ID (match with stored CategoryId in Campaign)
        //        if (!string.IsNullOrEmpty(categoryFilter) && int.TryParse(categoryFilter, out int categoryId))
        //        {
        //            campaigns = campaigns
        //                .Where(c => c.CategoryId == categoryId)
        //                .ToList();
        //        }

        //        // Pass selected category back to view
        //        ViewBag.SelectedCategoryId = categoryFilter;

        //        // Pagination
        //        int pageSize = 5;
        //        int pageNumber = page ?? 1;
        //        var pagedCampaigns = campaigns.ToPagedList(pageNumber, pageSize);

        //        ViewBag.SearchTerm = searchTerm;
        //        ViewBag.StatusFilter = statusFilter;
        //        // ViewBag.CategoryFilter = categoryFilter;
        //        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        //        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        //        //ViewBag.Categories = new SelectList(allCategories);

        //        return View(pagedCampaigns);

        //        //return View(campaigns);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
        //    }
        //}


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

        [HttpGet,ActionName("Delete")]
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

        [HttpGet, ActionName("Details")]
        public async Task<IActionResult> CampaignDetails(int id)
        {
            var campaign = await _campaign.GetCampaignById(id);
            return View(campaign);
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

        [HttpGet]
        public async Task<IActionResult> ViewContributors(int campaignId) // campaignId
        {
            try
            {
                var contributors = await _campaign.GetContributorsByCampaignId(campaignId);
                return View(contributors);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}