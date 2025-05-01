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

        public HomeController(ILogger<HomeController> logger, ICampaignsRepos campaign, IAdminDashboard adminDashboard) 
        {
            _logger = logger;
            _campaign = campaign;
            _adminDashboard = adminDashboard;
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
        
        public IActionResult Contact()
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
