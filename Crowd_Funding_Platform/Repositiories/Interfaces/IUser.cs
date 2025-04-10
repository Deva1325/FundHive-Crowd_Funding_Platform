using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IUser
    {
        Task<List<CreatorApplication>> GetAllCreatorsAsync();
        //Task<List<Contribution>> GetAllContributorsAsync();

        Task<CreatorApplication?> GetCreatorsById(int id);
        Task<bool> DeleteCreator(int id);
    }
}
