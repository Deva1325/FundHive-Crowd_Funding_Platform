using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes.Authorization
{
    public class AccountClassRepos : IAccountRepos
    {
        private readonly DbMain_CFS _dbMain_CFS;
        private readonly IEmailSenderRepos _emailSender;

        public AccountClassRepos(DbMain_CFS dbMain_CFS, IEmailSenderRepos emailSender)
        {
            _dbMain_CFS = dbMain_CFS;
            _emailSender = emailSender;
        }

        public async Task<object> AddUserRegister(User user, IFormFile? ImageFile)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            user.DateCreated = DateTime.Now;

            user.Otp = _emailSender.GenerateOtp();
            user.Otpexpiry = DateTime.Now.AddMinutes(5);
            user.EmailVerified = false;
            string subj = "OTP Verification!!!";
            await _emailSender.SendEmailAsync(user.Email, subj, user.Otp, "Registration");

            await _dbMain_CFS.Users.AddAsync(user);
            await _dbMain_CFS.SaveChangesAsync();
            return new { success = true, message = "Registration Sucessfull!!" };
        }

        public async Task<int?> GetUserIdByEmail(string email)
        {
            return await _dbMain_CFS.Users
             .Where(u => u.Email == email)
             .Select(u => (int?)u.UserId)
             .FirstOrDefaultAsync(); //Retrieves the first match or null

        }

        public async Task<bool> IsEmailExist(string email)
        {
            return await _dbMain_CFS.Users.AnyAsync(u => u.Email == email);

        }

        public async Task<bool> IsUsernameExist(string username)
        {
            return await _dbMain_CFS.Users.AnyAsync(u => u.Username == username);

        }

        public async Task<bool> OtpVerification(string Otp)
        {
            return await _dbMain_CFS.Users.AnyAsync(u => u.Otp == Otp && u.Otpexpiry > DateTime.Now);
        }

        public async Task<object> updateStatus(string Email)
        {
            var user = await _dbMain_CFS.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user != null)
            {
                user.EmailVerified = true;
                await _dbMain_CFS.SaveChangesAsync();
                return new { success = true, message = "Email verified successfully" };
            }

            return new { success = false, message = "Email not found" };
        }
    }
}
