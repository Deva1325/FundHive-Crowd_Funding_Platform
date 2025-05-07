using ClosedXML.Excel;
using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using System.Threading.Tasks;
using X.PagedList.Extensions;

namespace Crowd_Funding_Platform.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly ICategories _categories;

        public CategoriesController(ICategories categories, ISidebarRepos sidebar):base(sidebar)
        {
            _categories = categories;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        //public async Task<IActionResult> CategoriesList()
        //{
        //    var categories = await _categories.GetAllCategories();
        //    return View(categories);
        //}

        [HttpGet]
        public async Task<IActionResult> CategoriesList(string searchString, int? page)
        {
            string ISadmin = HttpContext.Session.GetString("IsAdmin_ses");
            if (ISadmin != "true")
            {
                return RedirectToAction("unAuthorized401", "Error");
            }

            var categories = await _categories.GetAllCategories();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories
                    .Where(c => c.Name != null &&
                                c.Name.ToLower().Contains(searchString.ToLower()))
                    .ToList();
            }

            // Pagination setup
            int pageSize = 5;
            int pageNumber = page ?? 1;
            var pagedCategories = categories.ToPagedList(pageNumber, pageSize);

            ViewBag.CurrentFilter = searchString;

            return View(pagedCategories);
        }


        [HttpGet]
        public async Task<IActionResult> ExportCategoriesToExcel()
        {
            try
            {
                var categories = await _categories.GetAllCategories();

                if (categories == null || !categories.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No category data available to generate the report."
                    });
                }


                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Categories");
                    var currentRow = 1;

                    // Header
                    worksheet.Cell(currentRow, 1).Value = "Category ID";
                    worksheet.Cell(currentRow, 2).Value = "Category Name";
                    worksheet.Cell(currentRow, 3).Value = "Total Contributions";
                    worksheet.Cell(currentRow, 4).Value = "Description";

                    var headerRange = worksheet.Range(currentRow, 1, currentRow, 4);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Data Rows
                    foreach (var cat in categories)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = cat.CategoryId;
                        worksheet.Cell(currentRow, 2).Value = cat.Name ?? "-";
                        worksheet.Cell(currentRow, 3).Value = cat.TotalContributions;
                        worksheet.Cell(currentRow, 4).Value = cat.Description ?? "-";

                        var rowRange = worksheet.Range(currentRow, 1, currentRow, 4);
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
                            "CategoriesReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the categories report.",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportCategoriesToPdf(string searchString)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId_ses");
                string isAdmin = HttpContext.Session.GetString("IsAdmin_ses");

                if (userId == null && isAdmin != "true")
                    return RedirectToAction("Login", "Account");

                var categories = await _categories.GetAllCategories();

                if (!string.IsNullOrEmpty(searchString))
                {
                    categories = categories
                        .Where(c => c.Name != null &&
                                    c.Name.ToLower().Contains(searchString.ToLower()))
                        .ToList();
                }

                if (categories == null || !categories.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No category data available to generate the report."
                    });
                }

                // Generate PDF using QuestPDF
                var document = new CategoriesList_PDF(categories);
                var pdfBytes = document.GeneratePdf();

                return File(pdfBytes, "application/pdf", "CategoriesReport.pdf");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the category report PDF.",
                    error = ex.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> SaveCategories(int? id)
        {
            string ISadmin = HttpContext.Session.GetString("IsAdmin_ses");
            if (ISadmin != "true")
            {
                return RedirectToAction("unAuthorized401", "Error");
            }

            if (id == null || id == 0)
                return View(new Category()); // Add Mode (Empty form)

            var category = await _categories.GetCategoryById(id.Value);
            if (category == null)
                return NotFound();

            return View(category); // Edit Mode (Prefilled form)
        }

        //[HttpPost]
        //public async Task<IActionResult> SaveCategories(Category category)
        //{
        //    //if (ModelState.IsValid)
        //    //{
        //        bool isNew = category.CategoryId == 0; // Check if it's a new category
        //        bool isSaved = await _categories.SaveCategory(category);

        //        if (isSaved)
        //        {
        //            return Json(new
        //            {
        //                success = true,
        //                message = isNew ? "Category added successfully!" : "Category updated successfully!"
        //            });
        //        }
        //       // return Json(new { success = false, message = "Failed to save category!" });
        //    //}

        //    return Json(new { success = false, message = "Validation failed! Please check your inputs." });
        //}

        [HttpPost]
        public async Task<IActionResult> SaveCategories(Category category)
        {
            //if (ModelState.IsValid)
            //{
                bool isNew = category.CategoryId == 0;  // Check if it's a new category

                // ✅ Ensure ID is properly passed and checked
                bool isSaved = await _categories.SaveCategory(category);

                if (isSaved)
                {
                    return Json(new
                    {
                        success = true,
                        message = isNew ? "Category added successfully!" : "Category updated successfully!"
                    });
                }

                return Json(new { success = false, message = "Failed to save category!" });
            //}

            //return Json(new { success = false, message = "Validation failed! Please check your inputs." });
        }


        //[HttpGet]
        //public async Task<IActionResult> SaveCategories(int? id)
        //{
        //    if (id == null || id == 0)
        //        return View(new Category()); // Add Mode (Empty form)

        //    var category = await _categories.GetCategoryById(id.Value);
        //    if (category == null)
        //        return NotFound();

        //    return View(category); // Edit Mode (Prefilled form)
        //}

        //[HttpPost]
        //public async Task<IActionResult> SaveCategories(Category category)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        bool isSaved = await _categories.SaveCategory(category);
        //        if (isSaved)
        //        {
        //            return Json(new
        //            {
        //                success = true,
        //                message = category.CategoryId == 0 ? "Category added successfully!" : "Category updated successfully!"
        //            });
        //        }
        //        return Json(new { success = false, message = "Failed to save category!" });
        //    }

        //    return Json(new { success = false, message = "Validation failed! Please check your inputs." });
        //}


        //[HttpDelete("DeleteCategory/{id}")]
        //public async Task<IActionResult> DeleteCategory(int id)
        //{
        //    var result = await _categories.DeleteCategory(id);

        //    if (result)
        //    {
        //        return Json(new { success = true, message = "Category deleted successfully!" });
        //    }

        //    return Json(new { success = false, message = "Failed to delete category." });
        //}

        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categories.DeleteCategory(id);
            return Json(new
            {
                success = result,
                message = result ? "Category deleted successfully!" : "Failed to delete category."
            });
        }

    }
}
