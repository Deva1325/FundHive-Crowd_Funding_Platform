using System.Diagnostics;
using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Crowd_Funding_Platform.Repositiories.Classes.Authorization
{
    public class LoginClassRepos : ILoginRepos
    {
        private readonly DbMain_CFS _dbMain_CFS;
        private readonly IEmailSenderRepos _emailSender;
        private readonly IMemoryCache _memoryCache;

        public LoginClassRepos(DbMain_CFS dbMain_CFS, IEmailSenderRepos emailSender, IMemoryCache memoryCache)
        {
            _dbMain_CFS = dbMain_CFS;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        public async Task<object> AuthenticateUser(string EmailOrUsername, string password)
        {
            const int maxAttempts = 5; //Maximum allowed attempts
            const int lockoutDurationSeconds = 300; //Lockout duration in seconds

            //Define cache keys for tracking attempts and lockout status
            var attemptKey = $"LoginAttempts_{EmailOrUsername}";
            var lockoutKey = $"Lockout_{EmailOrUsername}";

            //Fetching user by email or username
            var user = await _dbMain_CFS.Users.FirstOrDefaultAsync(u => u.Email == EmailOrUsername || u.Username == EmailOrUsername);

            if (user == null)
            {
                return new { success = false, message = "User not found" };
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isPasswordValid)
            { 
                _memoryCache.Remove(attemptKey);
                return new { success = true, message = "You are successfully logged in", email = user.Email };
            }

            //Check if the user is locked out
            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
            {
                var remainingTime = (int)(lockoutEndTime - DateTime.Now).TotalSeconds;
                return new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." };
            }

            int attempts = _memoryCache.GetOrCreate(attemptKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(lockoutDurationSeconds);
                return 0;
            });

            attempts++;
            _memoryCache.Set(attemptKey, attempts, TimeSpan.FromSeconds(lockoutDurationSeconds));

            //Lockout the user if max attempts reached
            if (attempts >= maxAttempts)
            {
                _memoryCache.Set(lockoutKey, DateTime.Now.AddSeconds(lockoutDurationSeconds), TimeSpan.FromSeconds(lockoutDurationSeconds));
                _memoryCache.Remove(attemptKey); //Reset attempts after lockout
                return new { success = false, message = $"Account is locked. Try again in {lockoutDurationSeconds} seconds." };

            }

            return new { success = false, message = "Invalid user id or pass" };
        }

        public async Task<object> ResetPassword(string creds, string newPassword)
        {
            var user = await _dbMain_CFS.Users.FirstOrDefaultAsync(u => u.Email == creds || u.Username == creds);
            //var user =  _dbMain_CFS.Users.FirstOrDefault(u=>u.Email==creds || u.Username==creds);

            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _dbMain_CFS.SaveChanges();
                return new { success = true, message = "Password Changed Successfully" };
            }
            return new { success = false, message = "User not found" };

        }

        public async Task<object> TokenSenderViaEmail(string email)
        {
            var user = await _dbMain_CFS.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new { success = false, message = "No user found with that email or username" };
            }

            //We are creating token variables here
            var resetToken = Guid.NewGuid().ToString();
            var expirationTime = DateTime.Now.AddDays(1); //Token expires in an hour

            //Store the token and expiration time in-memory
            _memoryCache.Set(resetToken, new { UserId = user.UserId, ExpirationTime = expirationTime }, expirationTime);

            //Create the password reset URL with the token
            var resetUrl = "http://localhost:5084/Account/ResetPassword?token=" + resetToken;

            //Send the reset email
            await _emailSender.SendEmailAsync(user.Email,user.Username, "Password Reset Request",
                $"Click <a href='{resetUrl}'>here</a> to reset your password.", "ForgotPassword");

            return new { success = true, message = "Password reset link sent to your email." };
        }

        public async Task<object> LogoutUser(string email)
        {
            var user = await _dbMain_CFS.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                return new { success = true, message = "You have been logged out successfully." };
            }

            return new { success = false, message = "Logout failed. User not found." };
        }
    }
}
