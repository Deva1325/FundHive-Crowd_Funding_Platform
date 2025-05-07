using System.Diagnostics;
using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICampaignsRepos _campaign;
        private readonly IAdminDashboard _adminDashboard;
        private readonly DbMain_CFS _CFS;

        public HomeController(ILogger<HomeController> logger, ICampaignsRepos campaign, IAdminDashboard adminDashboard,DbMain_CFS dbMain_CFS) 
        {
            _logger = logger;
            _campaign = campaign;
            _adminDashboard = adminDashboard;
            _CFS = dbMain_CFS;   
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomePageViewModel
            {
                Campaigns = _campaign.ShowCampaignCases() // OR ShowCampaignCases()           
            };

            TotalCounts total = new TotalCounts
            {
                TotalUsers = await _adminDashboard.GetTotalUsers(),
                TotalCampaigns = await _adminDashboard.GetTotalCampaigns(),
                TotalContributions = await _adminDashboard.GetTotalContributions(),
                TotalRaisedAmount = await _adminDashboard.GetTotalRaisedAmount()
            };

            ViewBag.Totals = total;
            return View(viewModel);  // pass one model with all data
        }
        
        
        [HttpGet]
        public IActionResult ContactUs()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        [HttpPost]
        public JsonResult ContactUs(TblContact contact)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(contact.Email) || string.IsNullOrWhiteSpace(contact.Message))
                {
                    return Json(new { success = false, message = "Please fill in all required fields." });
                }

                // Check if the user is logged in
                var userEmail = HttpContext.Session.GetString("UserEmail");
                var usernm = HttpContext.Session.GetString("UserName");
                if (userEmail != null && usernm!=null)
                {
                    contact.Email = userEmail; 
                    contact.Name = usernm;
                }

                contact.SubmittedAt = DateTime.Now;
                _CFS.TblContacts.Add(contact);
                _CFS.SaveChanges();

                return Json(new { success = true, message = "Thank you for contacting us. We will get back to you soon!" });
            }
            catch (Exception ex)
            {
                // Optional: Log exception
                return Json(new { success = false, message = "Something went wrong. Please try again later." });
            }
        }


        public IActionResult About()
        {
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
