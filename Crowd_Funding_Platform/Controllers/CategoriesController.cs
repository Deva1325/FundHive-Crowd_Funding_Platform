using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        public async Task<IActionResult> CategoriesList()
        {
            var categories = await _categories.GetAllCategories();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> SaveCategories(int? id)
        {
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
