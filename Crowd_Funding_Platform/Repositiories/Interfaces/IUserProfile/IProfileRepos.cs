using Crowd_Funding_Platform.Models;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Repositiories.Interfaces.IUserProfile
{
    public interface IProfileRepos
    {
        Task<User> GetAllUsersData(int userId);

        Task<object> EditProfile(User user, IFormFile? ImageFile);
    }
}
