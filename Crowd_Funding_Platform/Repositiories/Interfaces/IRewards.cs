using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IRewards
    {
        Task<List<Reward>> GetAllRewards();
        Task<Reward> GetRewardById(int rewardId);
        Task<bool> SaveReward(Reward reward);
        Task<bool> DeleteReward(int rewardId);
    }
}
