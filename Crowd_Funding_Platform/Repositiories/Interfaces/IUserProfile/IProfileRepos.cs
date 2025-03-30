using Crowd_Funding_Platform.Models;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Repositiories.Interfaces.IUserProfile
{
    public interface IProfileRepos
    {
        Task<User> GetAllUsersData(int userId);

        Task<User?> EditProfile(User user, IFormFile? ImageFile);
        Task<object> UpdateEmailVerification(User users);
        Task<bool> OtpVerification(string Otp);
        Task<object> updateStatus(string Email);
    }
}
