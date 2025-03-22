using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class CategoriesClassRepos : ICategories
    {
        private readonly DbMain_CFS _db;

        public CategoriesClassRepos(DbMain_CFS db)
        {
            _db = db;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await _db.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryById(int categoryId)
        {
            return await _db.Categories.FindAsync(categoryId);
        }

        //public async Task<bool> SaveCategory(Category category)
        //{
        //    try
        //    {
        //        bool isNew = category.CategoryId == 0; // Check if it's a new category

        //        if (isNew)
        //            _db.Categories.Add(category); // Add new
        //        else
        //            _db.Categories.Update(category); // Update existing

        //        await _db.SaveChangesAsync();
        //        return isNew; // Return true for new category, false for update
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public async Task<bool> SaveCategory(Category category)
        {
            try
            {
                if (category == null) return false;

                if (category.CategoryId == 0)
                {
                    await _db.Categories.AddAsync(category); // Add new
                }
                else
                {
                    var existingCategory = await _db.Categories.FindAsync(category.CategoryId);

                    if (existingCategory != null)
                    {
                        existingCategory.Name = category.Name;
                        existingCategory.Description = category.Description;

                        _db.Categories.Update(existingCategory);
                    }
                    else
                    {
                        return false;  // Return false if no category found to update
                    }
                }

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Current
        //public async Task<bool> SaveCategory(Category category)
        //{
        //    try
        //    {
        //        if (category.CategoryId == 0)
        //            await _db.Categories.AddAsync(category); // Add New
        //        else
        //            _db.Categories.Update(category); // Update Existing

        //        await _db.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //public async Task<bool> DeleteCategory(int categoryId)
        //{
        //    try
        //    {
        //        var category = await _db.Categories.FindAsync(categoryId);
        //        if (category == null) return false;

        //        _db.Categories.Remove(category);
        //        await _db.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}"); // For debugging
        //        return false;
        //    }
        //}

        public async Task<bool> DeleteCategory(int categoryId)
        {
            try
            {
                var category = await _db.Categories.FindAsync(categoryId);
                if (category == null) return false;

                _db.Categories.Remove(category);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
