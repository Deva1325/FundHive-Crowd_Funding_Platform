using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

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

            string originalPassword = user.PasswordHash;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            user.DateCreated = DateTime.Now;

            // ✅ Store plain password BEFORE calling AddUserAsync

            // Check if it's a Google user
            if (user.IsGoogleAccount == true)
            {
                user.EmailVerified = true;

                string userSubject = "🎉 Welcome - FundHive Login Info";

                // Pass data in a delimited format like "username|email|password"
                string userBody = $"{user.Username}|EMAIL:{user.Email}|PASSWORD:{originalPassword}";

                await _emailSender.SendEmailAsync(
                    user.Email,                   // toEmail
                    user.Username,                // userName
                    userSubject,                  // subject
                    userBody,                     // body (contains all values for template)
                    "GoogleRegistration"          // emailType
                );
            }
            else
            {
                user.Otp = _emailSender.GenerateOtp();
                user.Otpexpiry = DateTime.Now.AddMinutes(5);
                user.EmailVerified = false;
                string subj = "OTP Verification!!!";
                await _emailSender.SendEmailAsync(user.Email, user.Username, subj, user.Otp, "Registration");
            }

            if(ImageFile == null)
            {
                user.ProfilePicture = GenerateDefaultProfileImage(user.Username,user.UserId);
            }

            await _dbMain_CFS.Users.AddAsync(user);
            await _dbMain_CFS.SaveChangesAsync();
            return new { success = true, message = "Registration Sucessfull!!" };
        }


        public string GenerateDefaultProfileImage(string userName, int userId, bool generateOnly = false)
        {
            if (string.IsNullOrWhiteSpace(userName))
                userName = "U"; // fallback default

            string firstLetter = userName.Substring(0, 1).ToUpper();
            string safeUserName = userName.Replace(" ", "_"); // optional: sanitize
            string fileName = $"{safeUserName}_{userId}_profile.png";

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fullPath = Path.Combine(directoryPath, fileName);
            string relativePath = $"/ProfileImage/{fileName}";

            if (generateOnly)
            {
                return relativePath; // Just return the path, don't generate the file
            }

            // Always regenerate the image, regardless of existing file (to handle username change correctly)
            try
            {
                using (Bitmap bmp = new Bitmap(200, 200))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Gray);

                        using (Font font = new Font("Arial", 80, FontStyle.Bold, GraphicsUnit.Pixel))
                        using (SolidBrush textBrush = new SolidBrush(Color.White))
                        {
                            SizeF textSize = g.MeasureString(firstLetter, font);
                            PointF position = new PointF((bmp.Width - textSize.Width) / 2, (bmp.Height - textSize.Height) / 2);
                            g.DrawString(firstLetter, font, textBrush, position);
                        }
                    }

                    bmp.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate profile image: " + ex.Message);
            }

            return relativePath;
        }

        //old code
        //public string GenerateDefaultProfileImage(string userName)
        //{
        //    if (string.IsNullOrWhiteSpace(userName))
        //        userName = "U"; // Default to 'U' if username is empty

        //    string firstLetter = userName.Substring(0, 1).ToUpper();
        //    string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProfileImage");
        //    if (!Directory.Exists(directoryPath))
        //    {
        //        Directory.CreateDirectory(directoryPath);
        //    }

        //    string imagePath = Path.Combine(directoryPath, $"{userName}_profile.png");
        //    string relativePath = $"/ProfileImage/{userName}_profile.png"; // Path for DB storage

        //    int width = 200, height = 200;

        //    using (Bitmap bmp = new Bitmap(width, height))
        //    {
        //        using (Graphics g = Graphics.FromImage(bmp))
        //        {
        //            g.Clear(Color.Gray); // Set background color

        //            using (System.Drawing.Font font = new System.Drawing.Font("Arial", 80, FontStyle.Bold, GraphicsUnit.Pixel))
        //            using (SolidBrush textBrush = new SolidBrush(Color.White))
        //            {
        //                SizeF textSize = g.MeasureString(firstLetter, font);
        //                PointF position = new PointF((width - textSize.Width) / 2, (height - textSize.Height) / 2);
        //                g.DrawString(firstLetter, font, textBrush, position);
        //            }
        //        }

        //        bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
        //    }

        //    return relativePath;
        //}


        public async Task<string> fetchEmail(string cred)
        {
            return await _dbMain_CFS.Users.Where(u => u.Email == cred || u.Username == cred).Select(u => u.Email).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserDataByEmail(string email)
        {
            return await _dbMain_CFS.Users.FirstOrDefaultAsync(x => x.Email == email);
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

            if (user != null)  // Check for null before accessing properties
            {
                user.EmailVerified = true;
                await _dbMain_CFS.SaveChangesAsync();
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
