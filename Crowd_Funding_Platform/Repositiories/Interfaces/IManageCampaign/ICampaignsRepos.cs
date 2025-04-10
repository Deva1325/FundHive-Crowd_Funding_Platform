using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign
{
    public interface ICampaignsRepos
    {
        Task<List<CreatorApplication>> GetPendingCampaigns(); // Add this method
                                                              //Task<object> SaveCampaigns(Campaign campaign);
        //Task<(bool success, string message)> SaveCampaigns(Campaign campaign, int userId,IFormFile? ImageFile); // Modified to include userId
        Task<(bool success, string message)> SaveCampaigns(Campaign campaign, int userId, IFormFile? ThumbnailImage, List<IFormFile>? GalleryImages); // Modified to include userId

        Task<List<Campaign>> GetAllCampaigns();

        Task<Campaign?> GetCampaignById(int id);

        Task<CreatorApplication?> GetApplicationById(int id);

        Task<bool> DeleteCampaign(int id);

        Task<(bool success, string message)> ApproveCreator(int id);
        Task<(bool success, string message)> RejectCreator(int id);

        List<Campaign> ShowCampaignCases(); //Get All Campaign 
        Campaign DetailCampaignCases(int campaignId); //BY ID

        Task<List<Campaign>> GetCampaignsByCreator(int creatorId);

        Task<int> GetTotalContributors(int campaignId);
        Task<List<Contribution>> GetContributorsByCampaignId(int campaignId);



    }
}
