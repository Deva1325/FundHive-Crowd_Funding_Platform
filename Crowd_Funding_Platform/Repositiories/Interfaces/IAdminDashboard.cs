using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IAdminDashboard
    {
        Task<int> GetTotalUsers();
        Task<int> GetTotalCampaigns();
        Task<int> GetTotalContributions();
        Task<decimal> GetTotalRaisedAmount();
        Task<int> GetTotalCreatorApplications();
        Task<int> GetPendingCreatorApplications();
        Task<int> GetTotalPayments();
        Task<int> GetTotalRewards();


        //Charts
        Task<List<ChartDataPoint>> GetRaisedAmountPerWeek();
        Task<List<ChartDataPoint>> GetTop5Campaigns();
        Task<List<ChartDataPoint>> GetPaymentStatusDistribution();
        Task<List<ChartDataPoint>> GetContributionsByCategory();

        Task<List<Campaign>> GetLatestCampaigns();
        Task<List<CreatorApplication>> GetPendingCreatorApplications_Charts();
        Task<List<Contribution>> GetRecentContributions();

        Task<List<ChartDataPoint>> GetTop5Contributors();
    }

}
