using Crowd_Funding_Platform.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface ICategories
    {
        Task<List<Category>> GetAllCategories();
        Task<Category> GetCategoryById(int categoryId);
        Task<bool> SaveCategory(Category category);
        Task<bool> DeleteCategory(int categoryId);

    }
}
