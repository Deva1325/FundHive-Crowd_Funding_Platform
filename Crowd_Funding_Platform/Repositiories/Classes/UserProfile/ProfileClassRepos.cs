using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Crowd_Funding_Platform.Repositiories.Interfaces.IUserProfile;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace Crowd_Funding_Platform.Repositiories.Classes.UserProfile
{
    public class ProfileClassRepos : IProfileRepos
    {
        private readonly DbMain_CFS _CFS;
        private readonly IAccountRepos _acc;
        private readonly IEmailSenderRepos _emailSender;

        public ProfileClassRepos(DbMain_CFS dbMain_CFS, IAccountRepos acc, IEmailSenderRepos emailSender)
        {
            _CFS = dbMain_CFS;
            _acc = acc;
            _emailSender = emailSender;
        }
        public async Task<User?> EditProfile(User user, IFormFile? ImageFile)
        {
            var UpdateProfile = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (UpdateProfile == null)
            {
                return null;
            }

            // Handle Profile Picture Upload
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();

                if (allowedExtensions.Contains(fileExtension) && ImageFile.Length <= 1 * 1024 * 1024)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(UpdateProfile.ProfilePicture))
                    {
                        string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", UpdateProfile.ProfilePicture.TrimStart('/'));
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    UpdateProfile.ProfilePicture = $"/ProfileImage/{uniqueFileName}";
                }
                else
                {
                    throw new Exception("Invalid file type or file size exceeds 1MB.");
                }
            }

            UpdateProfile.Username = user.Username;
            UpdateProfile.PhoneNumber = user.PhoneNumber;
            UpdateProfile.FirstName = user.FirstName;
            UpdateProfile.LastName = user.LastName;
            UpdateProfile.Address = user.Address;
            UpdateProfile.ProfileBio = user.ProfileBio;
            UpdateProfile.Website = user.Website;
            UpdateProfile.InstagramLink = user.InstagramLink;
            UpdateProfile.FaceBookLink = user.FaceBookLink;

            await _CFS.SaveChangesAsync();
            return UpdateProfile;
        }

        public async Task<User> GetAllUsersData(int userId)
        {
            return await _CFS.Users.Where(u=>u.UserId==userId).FirstOrDefaultAsync();
        }

        public async Task<object> UpdateEmailVerification(User users)
        {
            // Fetch the existing user from the database based on email
            var existingUser = await _CFS.Users.FirstOrDefaultAsync(u => u.Email == users.Email);

            // Check if the user exists
            if (existingUser == null)
            {
                return new { success = false, message = "User not found" };
            }

            // Update OTP-related fields
            existingUser.Otp = _emailSender.GenerateOtp();
            existingUser.Otpexpiry = DateTime.Now.AddMinutes(5);
            existingUser.EmailVerified = false;

            string subj = "OTP Verification!!!";
            await _emailSender.SendEmailAsync(existingUser.Email, existingUser.Username , subj, existingUser.Otp, "Registration");

            // Save changes
            await _CFS.SaveChangesAsync();

            return new { success = true, message = "Check your email for the OTP verification" };

        }

        public async Task<bool> OtpVerification(string Otp)
        {
            return await _CFS.Users.AnyAsync(u => u.Otp == Otp && u.Otpexpiry > DateTime.Now);
        }

        public async Task<object> updateStatus(string Email)
        {
            var user = await _CFS.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user != null)  // Check for null before accessing properties
            {
                user.EmailVerified = true;
                await _CFS.SaveChangesAsync();
                return new { success = true, message = "Email verified successfully" };
            }
            else
            {
                // Return a message if the email is not found
                return new { success = false, message = "Email not found" };
            }
        }
    }
}
