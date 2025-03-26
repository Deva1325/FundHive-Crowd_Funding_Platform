using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
//using Crowd_Funding_Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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
        public async Task<IActionResult> AddUserRegister()
        {
            return View();
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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            string email = HttpContext.Session.GetString("LoginCred");
            if (!string.IsNullOrEmpty(email))
            {
                var result = await _loginRepos.LogoutUser(email); // Optional logging
            }
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
