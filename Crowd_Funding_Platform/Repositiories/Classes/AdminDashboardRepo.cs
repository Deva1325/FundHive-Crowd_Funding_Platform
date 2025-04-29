using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class AdminDashboardRepo : IAdminDashboard
    {
        private readonly DbMain_CFS _context;

        public AdminDashboardRepo(DbMain_CFS context)
        {
            _context = context;
        }

        public async Task<int> GetTotalUsers()
            => await _context.Users.CountAsync();

        public async Task<int> GetTotalCampaigns()
            => await _context.Campaigns.CountAsync();

        public async Task<int> GetTotalContributions()
            => await _context.Contributions.CountAsync();

        public async Task<decimal> GetTotalRaisedAmount()
            => await _context.Contributions.SumAsync(c => c.Amount);

        public async Task<int> GetTotalCreatorApplications()
            => await _context.CreatorApplications.CountAsync();

        public async Task<int> GetPendingCreatorApplications()
            => await _context.CreatorApplications.CountAsync(a => a.Status == "Pending");

        public async Task<int> GetTotalPayments()
            => await _context.PaymentDetails.CountAsync();

        public async Task<int> GetTotalRewards()
            => await _context.UserRewards.CountAsync();


        //Charts

        public async Task<List<ChartDataPoint>> GetRaisedAmountPerWeek()
        {
            var groupedData = await _context.Contributions
                .Where(c => c.PaymentStatus == "Success" && c.Date != null)
                .GroupBy(c => c.Date.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync(); // Fetch data before applying C#-only methods

            return groupedData.Select(g => new ChartDataPoint
            {
                Label = g.Date.ToShortDateString(), // Now safe to use in-memory
                Value = g.Total
            }).ToList();
        }


        public async Task<List<ChartDataPoint>> GetTop5Campaigns()
        {
            return await _context.Campaigns
                .OrderByDescending(c => c.RaisedAmount)
                .Take(5)
                .Select(c => new ChartDataPoint
                {
                    Label = c.Title,
                    Value = c.RaisedAmount ?? 0
                })
                .ToListAsync();
        }

        public async Task<List<ChartDataPoint>> GetPaymentStatusDistribution()
        {
            return await _context.Contributions
                .GroupBy(c => c.PaymentStatus)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToListAsync();
        }

        public async Task<List<ChartDataPoint>> GetContributionsByCategory()
        {
            return await _context.Campaigns
                .GroupBy(c => c.Category.Name)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.Contributions.Sum(c => c.Amount))
                })
                .ToListAsync();
        }

        public async Task<List<Campaign>> GetLatestCampaigns()
        {
            var data = await _context.Campaigns
                .Include(c => c.Creator)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CampaignId)
                .Take(5)
                .ToListAsync();


            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            foreach (var campaign in data)
            {
                // Set status based on StartDate and EndDate
                if (campaign.StartDate > today)
                    campaign.Status = "Upcoming";
                else if (campaign.StartDate <= today && campaign.EndDate >= today)
                    campaign.Status = "Ongoing";
                else if (campaign.EndDate < today)
                    campaign.Status = "Completed";
            }

            return data;
        }

        public async Task<List<CreatorApplication>> GetPendingCreatorApplications_Charts()
        {
            return await _context.CreatorApplications
        .Include(c => c.User)
        .Where(c => c.Status == "Pending" && c.User != null) // 👈 avoid null reference
        .OrderByDescending(c => c.SubmissionDate)
        .Take(5)
        .ToListAsync();
        }

        public async Task<List<Contribution>> GetRecentContributions()
        {
            return await _context.Contributions
                .Include(c => c.Campaign)
                .Include(c => c.Contributor)
                .OrderByDescending(c => c.Date)
                .Take(5)
                .ToListAsync();
        }

        public async Task<List<ChartDataPoint>> GetTop5Contributors()
        {
            return await _context.Contributions
                .Where(c => c.Contributor != null)
                .GroupBy(c => c.Contributor.Username)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Sum(c => c.Amount)
                })
                .OrderByDescending(g => g.Value)
                .Take(5)
                .ToListAsync();
        }


    }
}
