using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class AdminDashboardController : BaseController
    {
        private readonly IAdminDashboard _adminDashboard;

        public AdminDashboardController(IAdminDashboard adminDashboard, ISidebarRepos sidebar) : base(sidebar)
        {
            _adminDashboard = adminDashboard;
        }

        public async Task<IActionResult> Index()
        {
            string ISadmin = HttpContext.Session.GetString("IsAdmin_ses");

            if (ISadmin != "true")
            {
                return RedirectToAction("unAuthorized401", "Error");
            }

            var model = new AdminDashboardVM
            {
                TotalUsers = await _adminDashboard.GetTotalUsers(),
                TotalCampaigns = await _adminDashboard.GetTotalCampaigns(),
                TotalContributions = await _adminDashboard.GetTotalContributions(),


                TotalRaisedAmount = await _adminDashboard.GetTotalRaisedAmount(),
                TotalCreatorApplications = await _adminDashboard.GetTotalCreatorApplications(),
                PendingCreatorApplications = await _adminDashboard.GetPendingCreatorApplications(),
                TotalPayments = await _adminDashboard.GetTotalPayments(),
                TotalRewards = await _adminDashboard.GetTotalRewards(),

                RaisedPerWeek = await _adminDashboard.GetRaisedAmountPerWeek(),
                TopCampaigns = await _adminDashboard.GetTop5Campaigns(),
                PaymentStatusDistribution = await _adminDashboard.GetPaymentStatusDistribution(),
                ContributionsByCategory = await _adminDashboard.GetContributionsByCategory(),

                LatestCampaigns = await _adminDashboard.GetLatestCampaigns(),
                PendingApplications = await _adminDashboard.GetPendingCreatorApplications_Charts(),
                RecentContributions = await _adminDashboard.GetRecentContributions(),

                TopContributors = await _adminDashboard.GetTop5Contributors()
            };

            return View(model);
        }
    }
}
