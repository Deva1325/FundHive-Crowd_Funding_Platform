using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using X.PagedList.Extensions;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using Crowd_Funding_Platform.Repositiories.Classes;

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
            string ISadmin = HttpContext.Session.GetString("IsAdmin_ses");
            
            if (ISadmin != "true")
            {
                return RedirectToAction("unAuthorized401", "Error");
            }

            List<CreatorApplication> creators = await _user.GetAllCreatorsAsync();
            return View(creators);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCreatorsToExcel()
        {
            try
            {
                var creators = await _user.GetAllCreatorsAsync();

                if (creators == null || !creators.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No creator data available to generate the report."
                    });
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Creators");
                    var currentRow = 1;

                    // Header
                    worksheet.Cell(currentRow, 1).Value = "Username";
                    worksheet.Cell(currentRow, 2).Value = "Email";
                    worksheet.Cell(currentRow, 3).Value = "Phone Number";
                    worksheet.Cell(currentRow, 4).Value = "Submission Date";
                    worksheet.Cell(currentRow, 5).Value = "Status";
                    worksheet.Cell(currentRow, 6).Value = "Document Type";
                    worksheet.Cell(currentRow, 7).Value = "Admin Remarks";

                    var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Data Rows
                    foreach (var creator in creators)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = creator.User?.Username ?? "-";
                        worksheet.Cell(currentRow, 2).Value = creator.User?.Email ?? "-";
                        worksheet.Cell(currentRow, 3).Value = creator.User?.PhoneNumber ?? "-";
                        worksheet.Cell(currentRow, 4).Value = creator.SubmissionDate.HasValue
    ? creator.SubmissionDate.Value.ToString("dd MMM yyyy")
    : "-";

                        //worksheet.Cell(currentRow, 4).Value = creator.SubmissionDate.ToString("dd MMM yyyy");
                        worksheet.Cell(currentRow, 5).Value = creator.Status ?? "-";
                        worksheet.Cell(currentRow, 6).Value = creator.DocumentType ?? "-";
                        worksheet.Cell(currentRow, 7).Value = creator.AdminRemarks ?? "-";

                        var rowRange = worksheet.Range(currentRow, 1, currentRow, 7);
                        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "CreatorsReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the creators report.",
                    error = ex.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ExportCreatorsToPdf()
        {
            try
            {
                string isAdmin = HttpContext.Session.GetString("IsAdmin_ses");
                if (isAdmin != "true")
                    return RedirectToAction("Login", "Account");

                var creators = await _user.GetAllCreatorsAsync();

                if (creators == null || !creators.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No creator data available to generate the report."
                    });
                }

                var document = new CreatorsList_PDF(creators);
                var pdfBytes = document.GeneratePdf();

                return File(pdfBytes, "application/pdf", "CreatorsReport.pdf");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the creators report PDF.",
                    error = ex.Message
                });
            }
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

        [HttpGet]
        public async Task<IActionResult> ExportContributorsToExcel()
        {
            try
            {
                var contributors = await _user.GetAllContributorsAsync();

                if (contributors == null || !contributors.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No contributor data available to generate the report."
                    });
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Contributors");
                    var currentRow = 1;

                    // Header
                    worksheet.Cell(currentRow, 1).Value = "Username";
                    worksheet.Cell(currentRow, 2).Value = "Email";
                    worksheet.Cell(currentRow, 3).Value = "Phone";
                    worksheet.Cell(currentRow, 4).Value = "Campaign Title";
                    worksheet.Cell(currentRow, 5).Value = "Category";
                    worksheet.Cell(currentRow, 6).Value = "Amount";
                    worksheet.Cell(currentRow, 7).Value = "Date";
                    worksheet.Cell(currentRow, 8).Value = "Transaction ID";
                    worksheet.Cell(currentRow, 9).Value = "Payment Status";

                    var headerRange = worksheet.Range(currentRow, 1, currentRow, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Data Rows
                    foreach (var c in contributors)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = c.Contributor?.Username ?? "-";
                        worksheet.Cell(currentRow, 2).Value = c.Contributor?.Email ?? "-";
                        worksheet.Cell(currentRow, 3).Value = c.Contributor?.PhoneNumber ?? "-";
                        worksheet.Cell(currentRow, 4).Value = c.Campaign?.Title ?? "-";
                        worksheet.Cell(currentRow, 5).Value = c.Campaign?.Category?.Name ?? "-";
                        worksheet.Cell(currentRow, 6).Value = c.Amount.ToString("0.00");
                        worksheet.Cell(currentRow, 7).Value = c.Date.HasValue
    ? c.Date.Value.ToString("dd MMM yyyy")
    : "-";

                        worksheet.Cell(currentRow, 8).Value = c.TransactionId ?? "-";
                        worksheet.Cell(currentRow, 9).Value = c.PaymentStatus ?? "-";

                        var rowRange = worksheet.Range(currentRow, 1, currentRow, 9);
                        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "ContributorsReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the contributors report.",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportContributorsToPdf(string searchString, string category)
        {
            try
            {
                var contributors = await _user.GetAllContributorsAsync();

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    contributors = contributors
                        .Where(c => c.Contributor?.Username != null &&
                                    c.Contributor.Username.ToLower().Contains(searchString.ToLower()))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(category))
                {
                    contributors = contributors
                        .Where(c => c.Campaign?.Category?.Name == category)
                        .ToList();
                }

                if (!contributors.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No contributor data available to generate the report."
                    });
                }

                ViewBag.CurrentFilter = searchString;
                ViewBag.SelectedCategory = category;

                var document = new ContributorsList_PDF(contributors);
                var pdfBytes = document.GeneratePdf();


                return File(pdfBytes, "application/pdf", "ContributorsReport.pdf");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the contributors report.",
                    error = ex.Message
                });
            }
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

        [HttpGet]
        public async Task<IActionResult> MyContributions()
        {
            var contributions = await _user.MyContributions();
            return View(contributions);
        }

        [HttpGet]
        public async Task<IActionResult> ViewContributionHistory(int campaignId)
        {
            var contributions = await _user.GetContributionHistory(campaignId);
            return View(contributions);
        }

        //[HttpGet]
        //public IActionResult UsersList(string searchTerm, string roleFilter, int? monthFilter, int? page, bool isAjax = false)
        //{
        //    int pageSize = 5;
        //    int pageNumber = page ?? 1;

        //    var usersQuery = _CFS.Users.AsQueryable();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //        usersQuery = usersQuery.Where(u => u.Username.Contains(searchTerm));

        //    if (!string.IsNullOrEmpty(roleFilter))
        //    {
        //        if (roleFilter == "Creator")
        //            usersQuery = usersQuery.Where(u => u.IsCreatorApproved == true);
        //        else if (roleFilter == "Contributor")
        //            usersQuery = usersQuery.Where(u => u.IsCreatorApproved == false || u.IsCreatorApproved == null);
        //    }

        //    if (monthFilter.HasValue)
        //        usersQuery = usersQuery.Where(u => u.DateCreated.HasValue && u.DateCreated.Value.Month == monthFilter.Value);

        //    var pagedUsers = usersQuery.OrderByDescending(u => u.DateCreated).ToPagedList(pageNumber, pageSize);

        //    if (isAjax)
        //    {
        //        var jsonList = pagedUsers.Select(user => new
        //        {
        //            user.UserId,
        //            user.Username,
        //            user.Email,
        //            user.PhoneNumber,
        //            EmailVerified = user.EmailVerified.GetValueOrDefault(),
        //            IsCreatorApproved = user.IsCreatorApproved.GetValueOrDefault(),
        //            DateCreated = user.DateCreated?.ToString("dd-MM-yyyy"),
        //            ProfilePicture = string.IsNullOrEmpty(user.ProfilePicture)
        //                ? Url.Content("~/images/default-user.png")
        //                : user.ProfilePicture
        //        }).ToList();

        //        return Json(new
        //        {
        //            Users = jsonList,
        //            PageNumber = pagedUsers.PageNumber,
        //            PageCount = pagedUsers.PageCount
        //        });
        //    }

        //    // Normal View
        //    ViewBag.SearchTerm = searchTerm;
        //    ViewBag.RoleFilter = roleFilter;
        //    ViewBag.MonthFilter = monthFilter?.ToString();
        //    return View(pagedUsers);
        //}

        //[HttpGet]
        //public IActionResult UsersList(string searchTerm, string roleFilter, int? monthFilter, int? page)
        //{
        //    int pageSize = 5;
        //    int pageNumber = page ?? 1;

        //    var usersQuery = _CFS.Users.AsQueryable();

        //    // Search filter
        //    if (!string.IsNullOrEmpty(searchTerm))
        //        usersQuery = usersQuery.Where(u => u.Username.Contains(searchTerm));

        //    // Role filter
        //    if (!string.IsNullOrEmpty(roleFilter))
        //    {
        //        if (roleFilter == "Creator")
        //            usersQuery = usersQuery.Where(u => u.IsCreatorApproved == true);
        //        else if (roleFilter == "Contributor")
        //            usersQuery = usersQuery.Where(u => u.IsCreatorApproved == false || u.IsCreatorApproved == null);
        //    }

        //    // Month filter
        //    if (monthFilter.HasValue)
        //        usersQuery = usersQuery.Where(u => u.DateCreated.HasValue && u.DateCreated.Value.Month == monthFilter.Value);

        //    // Paged list (sync)
        //    var pagedUsers = usersQuery
        //        .OrderByDescending(u => u.DateCreated)
        //        .ToPagedList(pageNumber, pageSize);

        //    // Pass filters back to view
        //    ViewBag.SearchTerm = searchTerm;
        //    ViewBag.RoleFilter = roleFilter;
        //    ViewBag.MonthFilter = monthFilter?.ToString();

        //    return View(pagedUsers);
        //}


        [HttpGet]
        public async Task<IActionResult> UsersList(string searchTerm, string roleFilter, string monthFilter, int? page)
        {
            var allUsers = await _user.GetAllUsersList();

            // Search by name
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                allUsers = allUsers.Where(u =>
                    (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(searchTerm))
                ).ToList();
            }

            // Filter by role (Creator / Contributor)
            if (!string.IsNullOrEmpty(roleFilter))
            {
                if (roleFilter == "Creator")
                    allUsers = allUsers.Where(u => u.IsCreatorApproved == true).ToList();
                else if (roleFilter == "Contributor")
                    allUsers = allUsers.Where(u => u.IsCreatorApproved == false || u.IsCreatorApproved == null).ToList();
            }

            // Filter by month (JoinedOn)
            if (!string.IsNullOrEmpty(monthFilter) && int.TryParse(monthFilter, out int month))
            {
                allUsers = allUsers.Where(u => u.DateCreated?.Month == month).ToList();
            }

            // Pagination
            int pageSize = 5;
            int pageNumber = page ?? 1;
            var pagedUsers = allUsers.ToPagedList(pageNumber, pageSize);

            // Pass filters to ViewBag
            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.MonthFilter = monthFilter;

            return View(pagedUsers);
        }


        [HttpGet]
        public async Task<IActionResult> ExportUsersToExcel(string searchTerm, string roleFilter, string monthFilter)
        {
            try
            {
                var allUsers = await _user.GetAllUsersList();

                // Apply same filters as in UsersList
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    allUsers = allUsers.Where(u =>
                        (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(searchTerm))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(roleFilter))
                {
                    if (roleFilter == "Creator")
                        allUsers = allUsers.Where(u => u.IsCreatorApproved == true).ToList();
                    else if (roleFilter == "Contributor")
                        allUsers = allUsers.Where(u => u.IsCreatorApproved == false || u.IsCreatorApproved == null).ToList();
                }

                if (!string.IsNullOrEmpty(monthFilter) && int.TryParse(monthFilter, out int month))
                {
                    allUsers = allUsers.Where(u => u.DateCreated?.Month == month).ToList();
                }

                if (allUsers == null || !allUsers.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No user data available to export."
                    });
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Users");
                    var currentRow = 1;

                    // Header
                    worksheet.Cell(currentRow, 1).Value = "Username";
                    worksheet.Cell(currentRow, 2).Value = "First Name";
                    worksheet.Cell(currentRow, 3).Value = "Last Name";
                    worksheet.Cell(currentRow, 4).Value = "Email";
                    worksheet.Cell(currentRow, 5).Value = "Phone";
                    worksheet.Cell(currentRow, 6).Value = "Role";
                    worksheet.Cell(currentRow, 7).Value = "Joined On";

                    var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Data
                    foreach (var user in allUsers)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = user.Username ?? "-";
                        worksheet.Cell(currentRow, 2).Value = user.FirstName ?? "-";
                        worksheet.Cell(currentRow, 3).Value = user.LastName ?? "-";
                        worksheet.Cell(currentRow, 4).Value = user.Email ?? "-";
                        worksheet.Cell(currentRow, 5).Value = user.PhoneNumber ?? "-";
                        worksheet.Cell(currentRow, 6).Value = user.IsCreatorApproved == true ? "Creator" : "Contributor";
                        worksheet.Cell(currentRow, 7).Value = user.DateCreated?.ToString("dd MMM yyyy") ?? "-";

                        var rowRange = worksheet.Range(currentRow, 1, currentRow, 7);
                        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        rowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "UsersReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while exporting users.",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportUsersToPdf(string searchTerm, string roleFilter, string monthFilter)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId_ses");
                string isAdmin = HttpContext.Session.GetString("IsAdmin_ses");

                if (userId == null && isAdmin != "true")
                    return RedirectToAction("Login", "Account");

                var users = await _user.GetAllUsersList();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    users = users.Where(u =>
                        (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(searchTerm))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(roleFilter))
                {
                    if (roleFilter == "Creator")
                        users = users.Where(u => u.IsCreatorApproved == true).ToList();
                    else if (roleFilter == "Contributor")
                        users = users.Where(u => u.IsCreatorApproved == false || u.IsCreatorApproved == null).ToList();
                }

                if (!string.IsNullOrEmpty(monthFilter) && int.TryParse(monthFilter, out int month))
                {
                    users = users.Where(u => u.DateCreated?.Month == month).ToList();
                }

                if (users == null || !users.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No user data available to generate the report."
                    });
                }

                // Generate PDF using QuestPDF
                var document = new UsersList_PDF(users);
                var pdfBytes = document.GeneratePdf();

                return File(pdfBytes, "application/pdf", "UsersReport.pdf");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the user report PDF.",
                    error = ex.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> TopContributorsList()
        {
            var TopContributors = await _user.GetTop5Contributors();
            return View(TopContributors);
        }

        //[HttpGet]
        //public async Task<IActionResult> MyContributions()
        //{
        //    var contributions = await _user.GetMyContributions();
        //    return View(contributions);
        //}


    }
}
