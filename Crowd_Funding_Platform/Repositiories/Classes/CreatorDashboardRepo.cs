using Crowd_Funding_Platform.DTOs;
using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        public async Task<List<ContributionTrendDTO>> GetMonthlyContributions(int creatorId)
        {
            return await _context.Contributions
                .Where(c => c.Campaign.CreatorId == creatorId && c.Date.HasValue)
                .GroupBy(c => c.Date.Value.Month)
                .Select(g => new ContributionTrendDTO
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                    TotalAmount = g.Sum(x => x.Amount)
                })
                .ToListAsync();
        }

        public async Task<List<CampaignInsightDTO>> GetCampaignInsights(int creatorId)
        {
            return await _context.Campaigns
                .Where(c => c.CreatorId == creatorId)
                .Select(c => new CampaignInsightDTO
                {
                    CampaignTitle = c.Title,
                    AvgDonation = c.Contributions.Any() ? c.Contributions.Average(x => x.Amount) : 0,
                    TotalContributors = c.Contributions.Select(x => x.ContributorId).Distinct().Count(),
                    RaisedPercent = (c.RaisedAmount ?? 0m) / c.Requirement * 100
                })
                .ToListAsync();
        }


        public async Task<Dictionary<string, int>> GetCampaignStatusCounts(int creatorId)
        {
            return await _context.Campaigns
                .Where(c => c.CreatorId == creatorId)
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }


    }
}
