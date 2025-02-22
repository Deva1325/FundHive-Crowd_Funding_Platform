using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign
{
    public interface ICampaignsRepos
    {
        Task<List<CreatorApplication>> GetPendingCampaigns(); // Add this method
                                                              //Task<object> SaveCampaigns(Campaign campaign);

        Task<(bool success, string message)> SaveCampaigns(Campaign campaign, int userId,IFormFile? MediaUrl); // Modified to include userId

        Task<List<Campaign>> GetAllCampaigns();
    }
}
