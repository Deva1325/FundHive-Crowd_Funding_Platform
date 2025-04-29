using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class CreatorDashboardRepo : ICreatorDashboard
    {
        private readonly DbMain_CFS _context;

        public CreatorDashboardRepo(DbMain_CFS context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCampaignsByCreator(int creatorId)
        {
            return await _context.Campaigns.CountAsync(c => c.CreatorId == creatorId);
        }

        public async Task<int> GetActiveCampaignsByCreator(int creatorId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Campaigns.CountAsync(c =>
                c.CreatorId == creatorId && c.StartDate <= today && c.EndDate >= today);
        }

        public async Task<int> GetCompletedCampaignsByCreator(int creatorId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Campaigns.CountAsync(c =>
                c.CreatorId == creatorId && c.EndDate < today);
        }

        public async Task<decimal> GetTotalRaisedAmountByCreator(int creatorId)
        {
            return await _context.Campaigns
                .Where(c => c.CreatorId == creatorId)
                .SumAsync(c => c.RaisedAmount ?? 0);
        }

        public async Task<int> GetTotalContributorsByCreator(int creatorId)
        {
            return await _context.Contributions
                .Where(c => c.Campaign.CreatorId == creatorId)
                .Select(c => c.ContributorId)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<Campaign>> GetCampaignsForChart(int creatorId)
        {
            return await _context.Campaigns
                .Where(c => c.CreatorId == creatorId)
                .Include(c => c.Contributions)
                .ToListAsync();
        }
    }
}
