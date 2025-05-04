using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Crowd_Funding_Platform.Repositiories.Interfaces.IUserProfile;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Controllers
{
    public class ContributorProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private readonly IProfileRepos _profileRepos;
        private readonly DbMain_CFS _CFS;
        private readonly IEmailSenderRepos _emailSender;
        private readonly IAccountRepos _acc;

        public ContributorProfileController(IProfileRepos profileRepos, DbMain_CFS dbMain_CFS, IEmailSenderRepos emailSender, IAccountRepos accountRepos)
        {
            _profileRepos = profileRepos;
            _CFS = dbMain_CFS;
            _emailSender = emailSender;
            _acc = accountRepos;
        }

        [HttpGet]
        public async Task<IActionResult> ContributorProfile()
        {
            //Get the logged-in user Id from session
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _profileRepos.GetAllUsersData(userId.Value);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> ContributorEditProfile(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            User user = new User();

            if (id > 0)
            {
                user = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == id);
            }

            return View(user);

        }

        [HttpPost]
        public async Task<IActionResult> ContributorEditProfile(User users, IFormFile? ImageFile)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired. Please login again." });
            }

            users.UserId = userId.Value; // Ensure correct user ID is used 

            try
            {
                var updatedUser = await _profileRepos.EditProfile(users, ImageFile);

                if (updatedUser == null)
                {
                    return Json(new { success = false, message = "User not found!" });
                }

                // ✅ Update Session Values after Profile Update
                HttpContext.Session.SetString("UserName", updatedUser.Username ?? "");
                string userImagePath = string.IsNullOrEmpty(updatedUser.ProfilePicture) ? "/assets/default-user.png" : updatedUser.ProfilePicture;
                HttpContext.Session.SetString("UserImage", userImagePath);

                return Json(new { success = true, message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while updating profile. " + ex.Message });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> GenerateDefaultProfile()
        //{
        //    int? userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return Json(new { success = false, message = "Session expired. Please login again." });
        //    }

        //    var user = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        //    if (user == null)
        //    {
        //        return Json(new { success = false, message = "User not found." });
        //    }

        //    string relativePath = _acc.GenerateDefaultProfileImage(user.Username);

        //    // Remove old profile image if exists
        //    if (!string.IsNullOrEmpty(user.ProfilePicture))
        //    {
        //        string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture.TrimStart('/'));
        //        if (System.IO.File.Exists(oldImagePath))
        //        {
        //            System.IO.File.Delete(oldImagePath);
        //        }
        //    }

        //    user.ProfilePicture = relativePath;
        //    await _CFS.SaveChangesAsync();

        //    return Json(new { success = true, imagePath = relativePath });
        //}

        [HttpPost]
        public async Task<IActionResult> GenerateDefaultProfile([FromForm] string updatedUsername)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            var user = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Fallback to existing username if nothing passed
            string finalUsername = string.IsNullOrWhiteSpace(updatedUsername) ? user.Username : updatedUsername;

            // Remove all previously auto-generated images for this userId
            string imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");
            if (Directory.Exists(imageDirectory))
            {
                string searchPattern = $"*_{user.UserId}_profile.png";
                string[] matchingFiles = Directory.GetFiles(imageDirectory, searchPattern);

                foreach (string file in matchingFiles)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Error while cleaning old images: " + ex.Message });
                    }
                }
            }

            // Generate new image using updated username
            string finalRelativePath = _acc.GenerateDefaultProfileImage(finalUsername, user.UserId, generateOnly: false);

            // Save new profile picture path (username will be saved later on form submit)
            user.ProfilePicture = finalRelativePath;
            await _CFS.SaveChangesAsync();

            return Json(new { success = true, imagePath = finalRelativePath });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateEmailVerification([FromBody] User users)
        {
            try
            {
                if (users == null || string.IsNullOrEmpty(users.Email))
                {
                    return Json(new { success = false, message = "Invalid user or email is empty!" });
                }

                // Store Email in Session
                HttpContext.Session.SetString("UserEmail", users.Email);
                var email = HttpContext.Session.GetString("UserEmail");

                if (!string.IsNullOrEmpty(email))
                {
                    Console.WriteLine($"User Email Stored in Session: {email}");
                }

                // Call OTP generation and email sending service
                //var result = await _profile.UpdateEmailVerification(users);
                return Json(await _profileRepos.UpdateEmailVerification(users));


                // Ensure the session value is set correctly before redirecting
                //return Json(new { success = true, message = "OTP Sent!", redirectUrl = "/Profile/ProfileOTP" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet, ActionName("ProfileOTP")]
        public async Task<IActionResult> OtpCheck()
        {
            return View();
        }

        [HttpPost, ActionName("ProfileOTP")]
        public async Task<IActionResult> OtpCheck(User users)
        {
            try
            {
                if (await _acc.IsEmailExist(users.Email))
                {
                    if (await _acc.OtpVerification(users.Otp))
                    {
                        await _acc.updateStatus(users.Email);
                        return Json(new { success = true, message = "OTP Verified Successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "OTP Verification Failed :(" });

                    }
                }

                return Json(new { success = true, message = "Email not found!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString);
                return Json(new { success = false, message = "Unknown error occured" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResendOtp()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                var user = await _CFS.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    user.Otp = _emailSender.GenerateOtp();
                    user.Otpexpiry = DateTime.Now.AddMinutes(5);
                    await _emailSender.SendEmailAsync(user.Email, user.Username, "OTP Verification!!", user.Otp, "Registration");
                    await _CFS.SaveChangesAsync();
                    return Json(new { success = true, message = "OTP sent successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            return Json(new { success = false, message = "Email not found" });
        }
    }
}
