using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Classes.Authorization;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Crowd_Funding_Platform.Controllers
{//bs toh pchiii
    public class CampaignController : Controller
    {
        private readonly ICreatorApplicationRepos _creatorAppInterface;
        private readonly IMemoryCache _memoryCache;
        private readonly DbMain_CFS _dbMain1;

        public CampaignController(ICreatorApplicationRepos creatorApplication,IMemoryCache memoryCache,DbMain_CFS dbMain_CFS)
        {
            _creatorAppInterface = creatorApplication;
            _memoryCache = memoryCache;
            _dbMain1 = dbMain_CFS;
        }

        public IActionResult Index()
        {
            return View();           
        }

        [HttpGet]
        public async Task<IActionResult> ApplyForCreator()
        {
            ViewBag.DocumentTypes = new string[] { "Aadhar Card", "Voter ID", "Driving License", "PAN Card", "Passport" };

            var latestApplication = await _dbMain1.CreatorApplications
               .Where(x => x.UserId == HttpContext.Session.GetInt32("UserId_ses"))
               .OrderByDescending(x => x.SubmissionDate)
               .FirstOrDefaultAsync();

            var totalRequest = _dbMain1.CreatorApplications
               .Where(x => x.UserId == HttpContext.Session.GetInt32("UserId_ses"))
               .OrderByDescending(x => x.SubmissionDate);
            ViewBag.TotalRequest = 5 - totalRequest.Count();
            if (latestApplication != null)
            {
                
                if (latestApplication.Status == "Pending")
                {
                    ViewBag.creatorstatus = "You have already submitted a request. Please wait for admin approval.";
                }

                if (latestApplication.Status == "Approved")
                {
                    ViewBag.creatorstatus = "You are already approved as a creator.";
                }

                // If rejected – allow resubmission (no block)
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ApplyForCreator(CreatorApplication creatorApp, IFormFile? ImageFile)
        {
            try
            {
                string? SesEmail = HttpContext.Session.GetString("LoginCred");

                // Check if session is available
                if (string.IsNullOrEmpty(SesEmail))
                {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                }

                // Fetch User Details
                var user = await _dbMain1.Users.FirstOrDefaultAsync(u => u.Email == SesEmail);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Assign UserId from session user
                creatorApp.UserId = user.UserId;

                // Call service method and ensure it returns a valid JSON response
                var result = await _creatorAppInterface.ApplyForCreator(creatorApp, ImageFile);
                if (result == null)
                {
                    return Json(new { success = false, message = "Application submission failed. Please try again later." });
                }

                // Respect the actual repository result (whether success = false or true)
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> GetCreatorApplicationStatus(int userId)
        {
            ViewBag.CurrentUserId = userId;

            var latestApplication = await _dbMain1.CreatorApplications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.SubmissionDate)
                .FirstOrDefaultAsync();

            if (latestApplication == null)
                return Json(new { status = "None" });

            return Json(new { status = latestApplication.Status }); // Use status string directly
        }


        //[HttpPost]
        //public async Task<IActionResult> ApplyForCreator(CreatorApplication creatorApp, IFormFile? ImageFile)
        //{
        //    try
        //    {
        //        string SesEmail = HttpContext.Session.GetString("LoginCred");
        //        if (string.IsNullOrEmpty(SesEmail))
        //        {
        //            return Json(new { success = false, message = "Session expired. Please log in again." });
        //        }

        //        var user = await _dbMain1.Users.FirstOrDefaultAsync(u => u.Email == SesEmail);
        //        if (user == null)
        //        {
        //            return Json(new { success = false, message = "User not found." });
        //        }

        //        creatorApp.UserId = user.UserId;
        //        var result = await _creatorAppInterface.ApplyForCreator(creatorApp,ImageFile);

        //        return Json(result); // ✅ THIS FIXES THE ISSUE
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        //public async Task<JsonResult> GetCreatorApplicationStatus(int userId)
        //{


        //    var latestApplication = await _dbMain1.CreatorApplications
        //        .Where(x => x.UserId == userId)
        //        .OrderByDescending(x => x.SubmissionDate)
        //        .FirstOrDefaultAsync();

        //    if (latestApplication == null)
        //        return Json(new { status = "None" });

        //    return Json(new { status = latestApplication.Status.ToString() });
        //}



    }
}
