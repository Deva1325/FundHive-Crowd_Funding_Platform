using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

using Microsoft.AspNetCore.Authentication;

//using Crowd_Funding_Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Cryptography;
using System.Text;

namespace Crowd_Funding_Platform.Controllers
{
    public class AccountController : Controller
    {
        //private readonly IGoogleReCAPTCHAService _reCAPTCHAService;
        private readonly IAccountRepos _acc;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AccountController> _logger;
        private readonly DbMain_CFS _dbMain;
        private readonly ILoginRepos _loginRepos;
        private readonly IEmailSenderRepos _emailSender;

        public AccountController(IAccountRepos accountRepos, IEmailSenderRepos emailSender, IMemoryCache memoryCache, ILogger<AccountController> logger, ILoginRepos loginRepos, DbMain_CFS dbMain)
        {
            _acc = accountRepos;
            _memoryCache = memoryCache;
            _logger = logger;
            _dbMain = dbMain;
            _loginRepos = loginRepos;
            _emailSender = emailSender;
            //_reCAPTCHAService = reCAPTCHAService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet, ActionName("Registration")]
        public async Task<IActionResult> AddUserRegister(string? email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var tempPassword = GenerateTemporaryPassword(12);

                var model = new GoogleSignupModel
                {
                    Email = email,
                    PasswordHash = tempPassword,         // Use this directly in the form field
                    ConfirmPassword = tempPassword       // Optional: For auto-filling confirm password
                };

                ViewBag.GoogleData = model;
            }

            return View();
            //if (email != null)
            //{
            //    var model = new GoogleSignupModel { Email = email, temppassword = "hello12345678" };
            //    ViewBag.GoogleData = model;
            //}
            //return View();
        }

        private string GenerateTemporaryPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            StringBuilder password = new StringBuilder();

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (password.Length < length)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    password.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }

            return password.ToString();
        }


        //[HttpPost, ActionName("Registration")]
        //public async Task<IActionResult> AddUserRegister(User user, IFormFile? ImageFile)
        //{
        //    try
        //    {
        //        if (await _acc.IsUsernameExist(user.Username))
        //        {
        //            return Json(new { success = false, message = $"{user.Username} already exists." });
        //        }

        //        if (await _acc.IsEmailExist(user.Email))
        //        {
        //            return Json(new { success = false, message = $"{user.Email} already exists" });
        //        }

        //        // Store email in session
        //        HttpContext.Session.SetString("UserEmail", user.Email);

        //        var result = await _acc.AddUserRegister(user, ImageFile);

        //        if (result != null)
        //        {
        //            return Json(new { success = true, message = "User Registered successfully!" });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Failed to register user." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //}

        [HttpPost, ActionName("Registration")]
        public async Task<IActionResult> AddUserRegister(User user, IFormFile? ImageFile)
        {
            try
            {
                if (await _acc.IsUsernameExist(user.Username))
                {
                    return Json(new { success = false, message = $"{user.Username} already exists." });
                }

                if (await _acc.IsEmailExist(user.Email))
                {
                    return Json(new { success = false, message = $"{user.Email} already exists" });
                }

                //TempData["UserEmail"] = user.Email;
                HttpContext.Session.SetString("UserEmail", user.Email);

                if (ImageFile == null)
                {
                    user.ProfilePicture = _acc.GenerateDefaultProfileImage(user.Username);
                }

                //Get session value
                var email = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(email))
                {
                    Console.WriteLine($"User Email: {email}");
                }

                var result = await _acc.AddUserRegister(user, ImageFile);

                if (result != null)
                {
                    return Json(new { success = true, message = "User Registered successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to register user." });
                }

                // return Json(await _acc.AddUserRegister(user, ImageFile));
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OtpCheck()
        {
            string email = HttpContext.Session.GetString("UserEmail");

            var isVerified = _dbMain.Users
                .Where(x => x.Email == email)
                .Select(y => y.EmailVerified)
                .FirstOrDefault();

            if (isVerified == true)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> OtpCheck(User users)
        {
            try
            {
                // Get email from session
                //string? email = HttpContext.Session.GetString("UserEmail");
                //if (string.IsNullOrEmpty(email))
                //{
                //    return Json(new { success = false, message = "Session expired. Please try again." });
                //}

                //// Match session email with the provided one (optional validation)
                //if (!string.Equals(users.Email, email, StringComparison.OrdinalIgnoreCase))
                //{
                //    return Json(new { success = false, message = "Email mismatch. Please try again." });
                //}


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

                return Json(new { success = true, message = "Email not found!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OTP verification.");
                return Json(new { success = false, message = "An unknown error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResendOtp()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                var user = await _dbMain.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    user.Otp = _emailSender.GenerateOtp();
                    user.Otpexpiry = DateTime.Now.AddMinutes(5);
                    await _emailSender.SendEmailAsync(user.Email, user.Username, "OTP Verification!!", user.Otp, "Registration");
                    await _dbMain.SaveChangesAsync();
                    return Json(new { success = true, message = "OTP sent successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            return Json(new { success = false, message = "Email not found" });
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var model = new LoginModel();

            if (Request.Cookies.TryGetValue("RememberMe_Email", out string Emailvalue))
            {
                model.EmailOrUsername = Emailvalue;
                model.RememberMe = true;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            const int maxAttempts = 5; // Maximum allowed attempts
            const int lockoutDurationSeconds = 300; // Lockout duration in seconds

            var attemptKey = $"LoginAttempts_{login.EmailOrUsername}";
            var lockoutKey = $"Lockout_{login.EmailOrUsername}";

            if (_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
            {
                var remainingTime = (int)(lockoutEndTime - DateTime.Now).TotalSeconds;
                return Json(new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." });
            }


            ////current code to be kept 
            //var token = HttpContext.Request.Form["g-recaptcha-response"];

            //Console.WriteLine($"Received Token: {token}");

            //if (string.IsNullOrEmpty(token))
            //{
            //    return Json(new { success = false, message = "reCAPTCHA token is missing or invalid." });
            //}

            //// ✅ Verify reCAPTCHA token
            //var isCaptchaValid = await _reCAPTCHAService.VerifyToken(token);

            //if (!isCaptchaValid)
            //{
            //    return Json(new { success = false, message = "reCAPTCHA verification failed. Please try again." });
            //}


            var result = await _loginRepos.AuthenticateUser(login.EmailOrUsername, login.Password);

            if (((dynamic)result).success)
            {
                HttpContext.Session.SetString("LoginCred", login.EmailOrUsername);

                var user = await _dbMain.Users.FirstOrDefaultAsync(u => u.Email == login.EmailOrUsername || u.Username == login.EmailOrUsername);

                string redirectUrl = "/Home/Index"; // Default for contributors

                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId_ses", user.UserId);
                    HttpContext.Session.SetString("IsAdmin_ses", user.IsAdmin == true ? "true" : "false");
                    HttpContext.Session.SetString("IsCreatorApproved", user.IsCreatorApproved == true ? "true" : "false");

                    if (user.IsAdmin == true)
                    {
                        redirectUrl = "/Dashboard/Dashboard"; // Admin Dashboard
                    }
                    else if (user.IsCreatorApproved == true)
                    {
                        redirectUrl = "/Home/Index"; // Creator Dashboard
                    }
                }

                if (login.RememberMe)
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = true
                    };

                    Response.Cookies.Append("RememberMe_Email", login.EmailOrUsername, options);
                    Response.Cookies.Append("RememberMe_Password", login.Password, options);
                }
                else
                {
                    Response.Cookies.Delete("RememberMe_Email");
                    Response.Cookies.Delete("RememberMe_Password");
                }

                string email = await _acc.fetchEmail(login.EmailOrUsername);
                HttpContext.Session.SetString("UserEmail", email);

                var data = await _acc.GetUserDataByEmail(email);
                int id = data.UserId;
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetString("UserName", data.Username);

                if (data.ProfilePicture != null)
                {
                    HttpContext.Session.SetString("UserImage", data.ProfilePicture);
                }


                _memoryCache.Remove(attemptKey);

                return Json(new
                {
                    success = true,
                    message = "Login successful!",
                    role = user.IsAdmin == true ? "Admin" :
                   user.IsCreatorApproved == true ? "Creator" :
                   "Contributor",
                    redirectUrl // Ensure this is included in the response!
                });
            }
            return Json(new { success = false, message = "Invalid Login credentials. Please try again." });
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(User user)
        {
            HttpContext.Session.SetString("abc", user.Email);

            var res = await _loginRepos.TokenSenderViaEmail(user.Email);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token) || !_memoryCache.TryGetValue(token, out object tokenDate))
            {
                return Json(new { success = false, message = "Token is Invalid or Expired, please send another link" });
            }
            return View();
        }

        //OLD
        [HttpPost, ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAction(string PasswordHash)
        {
            //string user = HttpContext.Session.GetString("ForgotPwdEmail");
            //var res = await _loginRepos.ResetPassword(user,PasswordHash);
            //return Ok(res);    
            string userEmail = HttpContext.Session.GetString("abc");

            Console.WriteLine(userEmail);


            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "Session expired. Please request a new password reset." });
            }

            var result = await _loginRepos.ResetPassword(userEmail, PasswordHash);
            return Json(result);

        }

        public IActionResult Logout()
        {
            //string email = HttpContext.Session.GetString("LoginCred");
            //if (!string.IsNullOrEmpty(email))
            //{
            //    var result = await _loginRepos.LogoutUser(email); // Optional logging
            //}
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult GoogleDetails(string email)
        {
            //bool emailExists = _context.TblUsers.Any(u => u.Email == email);
            //if(emailExists)
            //{

            //}

            var model = new GoogleSignupModel { Email = email };
            return View(model);
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("Account/google-callback-view")]
        public IActionResult GoogleCallback()
        {
            return View("GoogleCallback", new object()); // This is your Razor view with JS
        }

        [HttpGet("Account/google-callback")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
            {
                return Json(new { success = false, message = "Google authentication failed. Please try again." });
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.Identity.Name;

            var user = await _dbMain.Users.FirstOrDefaultAsync(u => u.Email == email);
            string redirectUrl = "/Home/Index"; // Default for contributors


            //if (user != null && user.IsGoogleAccount == false)
            //{
            //    return Json(new { success = false, message = "This email is already registered manually. Use Email/Password to login." });
            //}

            if (user != null && user.IsGoogleAccount == true)
            {
       
                HttpContext.Session.SetInt32("UserId_ses", user.UserId);
                HttpContext.Session.SetString("IsAdmin_ses", user.IsAdmin == true ? "true" : "false");
                HttpContext.Session.SetString("IsCreatorApproved", user.IsCreatorApproved == true ? "true" : "false");
              
                if (user.IsAdmin == true)
                {
                    redirectUrl = "/Dashboard/Dashboard"; // Admin Dashboard
                }
                else if (user.IsCreatorApproved == true)
                {
                    redirectUrl = "/Home/Index"; // Creator Dashboard
                }

                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("LoginCred", email);

                var data = await _acc.GetUserDataByEmail(email);
                int id = data.UserId;
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetString("UserName", data.Username);

                if (data.ProfilePicture != null)
                {
                    HttpContext.Session.SetString("UserImage", data.ProfilePicture);
                }

                return Json(new
                {
                    success = true,
                    message = "Login successful!",
                    role = user.IsAdmin == true ? "Admin" :
                    user.IsCreatorApproved == true ? "Creator" :
                   "Contributor",
                    redirectUrl // Ensure this is included in the response!
                });
            }

            // New Google user
            return Json(new
            {
                success = true,
                message = "Google account not found. Please complete your details.",
                redirectUrl = Url.Action("Registration", "Account", new { email })
            });
        }

        [HttpPost]
        public async Task<IActionResult> GoogleDetails(GoogleSignupModel model)
        {
            // Check if username already exists
            bool usernameExists = _dbMain.Users.Any(u => u.Username == model.Username);
            if (usernameExists)
            {
                return Json(new { success = false, message = "Username Already Exists" });
            }

            // Create temp password
            string tempPassword = Guid.NewGuid().ToString();

            // Create user
            var user = new User
            {
                Email = model.Email,
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                IsGoogleAccount = true,
                EmailVerified = true,
                DateCreated = DateTime.UtcNow
                //VerificationStatus = "Pending"
            };

            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            //HttpContext.Session.SetInt32("UserRoleId", user.Role);


            _dbMain.Users.Add(user);
            await _dbMain.SaveChangesAsync();
            TempData["google-toast"] = "Account Created Successfully using Google!";
            TempData["google-toastType"] = "success";

            return Json(new { success = true, message = "Account Created Successfully" });
        }


    }
}
