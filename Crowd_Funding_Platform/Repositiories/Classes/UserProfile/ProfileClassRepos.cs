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
            var UpdateProfile = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (UpdateProfile == null)
            {
                return new { success = false, message = "User not found" };
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
                    return new { success = false, message = "Invalid file type or file size exceeds 1MB." };
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
            return new { success = true, message = "Your profile has been updated successfully" };
        }

        public async Task<User> GetAllUsersData(int userId)
        {
            return await _CFS.Users.Where(u=>u.UserId==userId).FirstOrDefaultAsync();
        }

    }
}
