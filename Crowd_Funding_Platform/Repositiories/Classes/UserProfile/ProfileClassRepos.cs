using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IUserProfile;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace Crowd_Funding_Platform.Repositiories.Classes.UserProfile
{
    public class ProfileClassRepos : IProfileRepos
    {
        private readonly DbMain_CFS _CFS;

        public ProfileClassRepos(DbMain_CFS dbMain_CFS)
        {
            _CFS = dbMain_CFS;
        }
        public async Task<object> EditProfile(User user, IFormFile? ImageFile)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetAllUsersData(int userId)
        {
            return await _CFS.Users.Where(u=>u.UserId==userId).FirstOrDefaultAsync();
        }

    }
}
