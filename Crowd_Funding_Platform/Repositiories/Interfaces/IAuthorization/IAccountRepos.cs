using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization
{
    public interface IAccountRepos
    {
        Task<object> AddUserRegister(User user, IFormFile? ImageFile);

        Task<bool> IsUsernameExist(string username);
        Task<bool> IsEmailExist(string email);
        Task<int?> GetUserIdByEmail(string email);

        Task<bool> OtpVerification(string Otp);

        Task<object> updateStatus(string Email);
    }
}
