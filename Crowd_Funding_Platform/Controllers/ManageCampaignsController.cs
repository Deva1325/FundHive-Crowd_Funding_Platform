﻿using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Classes;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Crowd_Funding_Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using QuestPDF.Fluent;


namespace Crowd_Funding_Platform.Controllers
{
    public class ManageCampaignsController : BaseController
    {
        private readonly ICampaignsRepos _campaign;
        private readonly DbMain_CFS _CFS;
        private readonly IActivityRepository _activityRepository;
        private readonly CampaignAiService _campaignAiService;

        private readonly OpenRouterService _openRouterService;

        public ManageCampaignsController(ICampaignsRepos campaign, DbMain_CFS dbMain_CFS, ISidebarRepos sidebar, IActivityRepository activityRepository, OpenRouterService openRouterService, CampaignAiService campaignAiService) : base(sidebar)
        {
            _campaign = campaign;
            _CFS = dbMain_CFS;
            _activityRepository = activityRepository;
            _openRouterService = openRouterService;
            _campaignAiService = campaignAiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ShowCampaignCases()
        {
            var campaigns = _campaign.ShowCampaignCases();

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // Set status and filter to Ongoing only
            var ongoingCampaigns = campaigns
                .Where(c =>
                {
                    if (c.StartDate > today)
                    {
                        c.Status = "Upcoming";
                        return false;
                    }
                    else if (c.StartDate <= today && c.EndDate >= today)
                    {
                        c.Status = "Ongoing";
                        return true;
                    }
                    else
                    {
                        c.Status = "Completed";
                        return false;
                    }
                })
                .ToList();

            return View(ongoingCampaigns); // ✅ return only ongoing
        }

        //[HttpGet]
        //public async Task<IActionResult> ShowCampaignCases()
        //{
        //    var campaigns = _campaign.ShowCampaignCases();
        //    return View(campaigns);
        //}

        [HttpGet]
        public async Task<IActionResult> DetailCampaignCases(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserEmail = email;

            var username = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = username;

            var campaign = _campaign.DetailCampaignCases(id);

            if (campaign == null)
            {
                return NotFound();
            }

            return View(campaign);
        }

        public async Task<IActionResult> PendingCampaigns()
        {
            string ISadmin = HttpContext.Session.GetString("IsAdmin_ses");
            if (ISadmin != "true")
            {
                return RedirectToAction("unAuthorized401", "Error");
            }

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
            if (id > 0)
            {
                campaign = await _CFS.Campaigns.FirstOrDefaultAsync(c => c.CampaignId == id);
                //campaign = campaign = await _CFS.Campaigns.Include(c => c.CampaignImages).FirstOrDefaultAsync(c => c.CampaignId == id);
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

                // ✅ Check if this is a new campaign BEFORE save
                bool isNew = campaign.CampaignId == 0;


                // ✅ Prevent editing non-upcoming campaigns
                if (!isNew)
                {
                    var existing = await _campaign.GetCampaignById(campaign.CampaignId);
                    if (existing != null && existing.Status != "Upcoming")
                    {
                        return Json(new { success = false, message = "Only 'Upcoming' campaigns can be edited." });
                    }
                }
              

                // ✅ Determine status based on start/end date
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

                //// Call OpenRouter API to generate campaign content (for title and description)
                //string aiResult = await _openRouterService.GenerateCampaignAsync(campaign.Title);
                //campaign.Description = aiResult;  // Assuming the result is used for campaign description

                // ✅ Call repository to save
                var result = await _campaign.SaveCampaigns(campaign, userId.Value, ImageFile, GalleryImages);


                // ✅ Use the earlier isNew flag for logging
                string activityType = isNew ? "Add" : "Update";
                string description = $"User with ID {userId.Value} performed '{activityType}' on campaign titled '{campaign.Title}'.";

                _activityRepository.AddNewActivity(
                    userId: userId.Value,
                    activityType: $"Campaign {activityType}",
                    description: description,
                    tableName: "Campaigns",
                    recordId: campaign.CampaignId
                );
                //if (!result.success)
                //{
                //    return Json(new { success = false, message = result.message });
                //}

                return Json(new { success = true, message = "Campaign saved successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // or log it properly
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


        [HttpGet]
        public async Task<IActionResult> CampaignsList(string searchTerm, string categoryFilter, string statusFilter, DateOnly? startDate, DateOnly? endDate, int? page)
        {
            try
            {
                string ISadmin = HttpContext.Session.GetString("IsAdmin_ses");
                string IScreator = HttpContext.Session.GetString("IsCreatorApproved");
                if (ISadmin != "true" && IScreator != "true")
                {
                    return RedirectToAction("unAuthorized401", "Error");
                }


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

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        campaigns = campaigns
                            .Where(c => c.Status != null && c.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    // Add total contributors
                    campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId);
                    //campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId) ?? 0;

                }

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
                ViewBag.SelectedStatus = statusFilter;

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
        public async Task<IActionResult> ExportCampaignsToPdf(string searchTerm, string categoryFilter, DateOnly? startDate, DateOnly? endDate)
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
                    if (campaign.StartDate > today)
                        campaign.Status = "Upcoming";
                    else if (campaign.StartDate <= today && campaign.EndDate >= today)
                        campaign.Status = "Ongoing";
                    else if (campaign.EndDate < today)
                        campaign.Status = "Completed";

                    campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId);
                }

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    campaigns = campaigns
                        .Where(c => c.Title != null &&
                                    c.Title.ToLower().Contains(searchTerm.ToLower()))
                        .ToList();
                }

                if (startDate.HasValue)
                {
                    campaigns = campaigns.Where(c => c.StartDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    campaigns = campaigns.Where(c => c.EndDate <= endDate.Value).ToList();
                }

                if (!string.IsNullOrEmpty(categoryFilter) && int.TryParse(categoryFilter, out int categoryId))
                {
                    campaigns = campaigns
                        .Where(c => c.CategoryId == categoryId)
                        .ToList();
                }

                if (campaigns == null || !campaigns.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No campaign data available to generate the report."
                    });
                }

                // Generate PDF using QuestPDF
                var document = new CampaignsList_PDF(campaigns);
                var pdfBytes = document.GeneratePdf();

                // Send Email with PDF Attachment
                var user = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user != null)
                {
                //    var subject = "Campaign Report Downloaded";
                //    var body = $@"
                //Hi {user.Username},<br><br>
                //Your <b>Campaign Report</b> has been successfully generated on 
                //<b>{DateTime.Now:dd MMM yyyy HH:mm}</b>.<br><br>
                //The report is attached to this email.<br><br>
                //Regards,<br>
                //FundHive Team";

                //    var attachmentFileName = "CampaignReport.pdf";

                //    await _emailSender.SendEmailWithAttachmentAsync(
                //        user.Email,
                //        subject,
                //        body,
                //        pdfBytes,
                //        attachmentFileName
                //    );
                }

                return File(pdfBytes, "application/pdf", "CampaignReport.pdf");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the campaign report PDF.",
                    error = ex.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ExportCampaignsToExcel(string searchTerm, string categoryFilter, DateOnly? startDate, DateOnly? endDate)
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
                    if (campaign.StartDate > today)
                        campaign.Status = "Upcoming";
                    else if (campaign.StartDate <= today && campaign.EndDate >= today)
                        campaign.Status = "Ongoing";
                    else if (campaign.EndDate < today)
                        campaign.Status = "Completed";

                    campaign.TotalContributors = await _campaign.GetTotalContributors(campaign.CampaignId);
                }

                // Filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    campaigns = campaigns
                        .Where(c => c.Title != null && c.Title.ToLower().Contains(searchTerm.ToLower()))
                        .ToList();
                }

                if (startDate.HasValue)
                {
                    campaigns = campaigns.Where(c => c.StartDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    campaigns = campaigns.Where(c => c.EndDate <= endDate.Value).ToList();
                }

                if (!string.IsNullOrEmpty(categoryFilter) && int.TryParse(categoryFilter, out int categoryId))
                {
                    campaigns = campaigns.Where(c => c.CategoryId == categoryId).ToList();
                }

                // Excel creation
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Campaigns");
                    var currentRow = 1;

                    // Header
                    worksheet.Cell(currentRow, 1).Value = "Title";
                    worksheet.Cell(currentRow, 2).Value = "Requirement Amount";
                    worksheet.Cell(currentRow, 3).Value = "Raised Amount";
                    worksheet.Cell(currentRow, 4).Value = "Start Date";
                    worksheet.Cell(currentRow, 5).Value = "End Date";
                    worksheet.Cell(currentRow, 6).Value = "Status";
                    worksheet.Cell(currentRow, 7).Value = "Total Contributors";

                    var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Data rows
                    foreach (var campaign in campaigns)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = campaign.Title ?? "-";
                        worksheet.Cell(currentRow, 2).Value = campaign.Requirement;
                        worksheet.Cell(currentRow, 3).Value = campaign.RaisedAmount;
                        worksheet.Cell(currentRow, 4).Value = campaign.StartDate.ToString("dd MMM yyyy");
                        worksheet.Cell(currentRow, 5).Value = campaign.EndDate.ToString("dd MMM yyyy");
                        worksheet.Cell(currentRow, 6).Value = campaign.Status ?? "-";
                        worksheet.Cell(currentRow, 7).Value = campaign.TotalContributors;

                        var rowRange = worksheet.Range(currentRow, 1, currentRow, 7);
                        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "CampaignsReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error exporting campaigns: " + ex.Message });
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
            var adminId = HttpContext.Session.GetInt32("UserId_ses") ?? 0;

            // ✅ Get the application details to fetch user info
            var application = await _campaign.GetApplicationById(id); // You should have this method
            if (application == null)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            var result = await _campaign.ApproveCreator(id);

            // ✅ Prepare and log activity
            string desc = $"Admin with ID {adminId} approved creator application of user ID {application.UserId}.";
            _activityRepository.AddNewActivity(
                adminId,
                "Approve Creator",
                desc,
                "CreatorApplications",
                id
            );

            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> RejectCreator(int id)
        {

            var adminId = HttpContext.Session.GetInt32("UserId_ses") ?? 0;

            var application = await _campaign.GetApplicationById(id);
            if (application == null)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            var result = await _campaign.RejectCreator(id);

            string desc = $"Admin with ID {adminId} rejected creator application of user ID {application.UserId}.";
            _activityRepository.AddNewActivity(
                adminId,
                "Reject Creator",
                desc,
                "CreatorApplications",
                id
            );


            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet, ActionName("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var deltCam = await _campaign.GetCampaignById(id);
            return View(deltCam);
        }

        // [HttpGet]
        // public async Task<IActionResult> DeleteConfirmed(int id)
        // {
        //     try
        //     {
        //         // ✅ Get userId from session (works for both admin and creator)
        //         var userId = HttpContext.Session.GetInt32("UserId_ses") ?? 0;

        //         var campaign = await _campaign.GetCampaignById(id);
        //         if (campaign == null)
        //         {
        //             return Json(new { success = false });
        //         }

        //         var result = await _campaign.DeleteCampaign(id);

        //          //✅ Log the delete action
        //                  string activityType = "Delete";
        //         string description = $"User with ID {userId} deleted campaign titled '{campaign.Title}'.";

        //         _activityRepository.AddNewActivity(
        //    userId: userId,
        //    activityType: $"Campaign {activityType}",
        //    description: description,
        //    tableName: "Campaigns",
        //    recordId: id
        //);
        //         return Json(new { success = true });
        //     }
        //     catch (Exception ex)
        //     {
        //         return Json(new { success = false, message = ex.Message });
        //     }
        // }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // ✅ Get userId from session (works for both admin and creator)
                var userId = HttpContext.Session.GetInt32("UserId_ses") ?? 0;

                // ✅ Fetch campaign before deleting (for title + check)
                var campaign = await _campaign.GetCampaignById(id);
                if (campaign == null)
                {
                    return Json(new { success = false });
                    // return NotFound();
                }

                var DelCam = await _campaign.DeleteCampaign(id);

                // ✅ Log the delete action
                string activityType = "Delete";
                string description = $"User with ID {userId} deleted campaign titled '{campaign.Title}'.";

                _activityRepository.AddNewActivity(
           userId: userId,
           activityType: $"Campaign {activityType}",
           description: description,
           tableName: "Campaigns",
           recordId: id
       );
                return Json(new { success = true });
                //return RedirectToAction("CampaignsList", "ManageCampaigns");
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
            //var campaign = await _campaign.GetCampaignById(id);
            var campaign = await _CFS.Campaigns.Include(c => c.CampaignImages).FirstOrDefaultAsync(c => c.CampaignId == id);
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

       
        //[HttpPost]
        //public async Task<IActionResult> GenerateAiTitle(string keyword)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(keyword))
        //        {
        //            return Json(new { success = false, message = "Keyword is required." });
        //        }

        //        var (title, description, category) = await _openRouterService.GenerateCampaignFullAsync(keyword);

        //        return Json(new { success = true, title, description, category });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "AI generation failed: " + ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> GenerateAiTitle(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return BadRequest("Category is required");

            var result = await _campaignAiService.GenerateCampaignAsync(category);

            if (!result.Success)
                return Json(new { success = false, error = result.ErrorMessage });

            return Json(new
            {
                success = true,
                title = result.Title,
                description = result.Description,
                category=result.Category,
                source = result.Source
            });
        }


    }
}

//[HttpPost]
//public async Task<IActionResult> GenerateAiTitle(string keyword)
//{
//    try
//    {
//        if (string.IsNullOrWhiteSpace(keyword))
//        {
//            return Json(new { success = false, message = "Keyword is required." });
//        }

//        string title = await _openRouterService.GenerateCampaignAsync(keyword);

//        return Json(new { success = true, title });
//    }
//    catch (Exception ex)
//    {
//        return Json(new { success = false, message = "AI generation failed: " + ex.Message });
//    }
//}










//    public async Task<string> GenerateTextAsync(string input)
//    {
//        var formContent = new FormUrlEncodedContent(new[]
//        {
//    new KeyValuePair<string, string>("text", input)
//});

//        var response = await _httpClient.PostAsync("https://api.deepai.org/api/text-generator", formContent);

//        if (!response.IsSuccessStatusCode)
//        {
//            var error = await response.Content.ReadAsStringAsync();
//            throw new Exception($"DeepAI error: {response.StatusCode} - {error}");
//        }

//        var responseString = await response.Content.ReadAsStringAsync();
//        using var jsonDoc = JsonDocument.Parse(responseString);
//        return jsonDoc.RootElement.GetProperty("output").GetString();
//    }


//    public class CampaignIdeaRequest
//    {
//        public string Category { get; set; }
//        public string Requirement { get; set; }
//    }


//[HttpPost]
//public async Task<IActionResult> GenerateCampaignIdea([FromBody] CampaignIdeaRequest request, [FromServices] OpenAIService openAI)
//{
//    try
//    {
//        var result = await openAI.GenerateCampaignContent(request.Category, request.Requirement);
//        return Json(new { success = true, content = result });
//    }
//    catch (Exception ex)
//    {
//        return Json(new { success = false, message = "AI error: " + ex.Message });
//    }
//}

//public class CampaignIdeaRequest
//{
//    public string Category { get; set; }
//    public string Requirement { get; set; }
//}