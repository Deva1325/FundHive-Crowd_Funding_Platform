using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IUser
    {
        Task<List<CreatorApplication>> GetAllCreatorsAsync();
        Task<List<Contribution>> GetAllContributorsAsync();

        Task<CreatorApplication?> GetCreatorsById(int id);
        Task<bool> DeleteCreator(int id);
        //Task<List<Contribution>> GetMyContributions();
        Task<List<Contribution>> MyContributions();
        Task<List<Contribution>> GetContributionHistory(int campaignId);

        Task<List<User>> GetAllUsersList();
    }
}
