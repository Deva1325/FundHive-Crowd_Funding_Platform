using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Crowd_Funding_Platform.Controllers
{
    public class CreatorDashboardController : BaseController
    {
        private readonly ICreatorDashboard _dashboard;

        public CreatorDashboardController(ICreatorDashboard dashboard, ISidebarRepos sidebar) : base(sidebar)
        {
            _dashboard = dashboard;
        }

        
        public async Task<IActionResult> Index()
        {
            int? creatorId = HttpContext.Session.GetInt32("UserId_ses");
            if (creatorId == null)
                return RedirectToAction("Login", "Account");

            var model = new CreatorDashboardVM
            {
                TotalCampaigns = await _dashboard.GetTotalCampaignsByCreator(creatorId.Value),
                ActiveCampaigns = await _dashboard.GetActiveCampaignsByCreator(creatorId.Value),
                CompletedCampaigns = await _dashboard.GetCompletedCampaignsByCreator(creatorId.Value),
                TotalRaisedAmount = await _dashboard.GetTotalRaisedAmountByCreator(creatorId.Value),
                TotalContributors = await _dashboard.GetTotalContributorsByCreator(creatorId.Value),
                ChartCampaigns = await _dashboard.GetCampaignsForChart(creatorId.Value)
            };

            return View(model);
        }
    }
}
