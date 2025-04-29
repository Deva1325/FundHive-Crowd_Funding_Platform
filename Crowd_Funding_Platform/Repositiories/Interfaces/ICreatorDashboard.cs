using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface ICreatorDashboard
    {
        Task<int> GetTotalCampaignsByCreator(int creatorId);
        Task<int> GetActiveCampaignsByCreator(int creatorId);
        Task<int> GetCompletedCampaignsByCreator(int creatorId);
        Task<decimal> GetTotalRaisedAmountByCreator(int creatorId);
        Task<int> GetTotalContributorsByCreator(int creatorId);
        Task<List<Campaign>> GetCampaignsForChart(int creatorId);
    }

}
