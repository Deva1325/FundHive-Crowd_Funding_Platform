using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IUserRewardRepository
    {
        Task<List<Reward>> GetEarnedRewardsAsync(int userId);

    }
}
