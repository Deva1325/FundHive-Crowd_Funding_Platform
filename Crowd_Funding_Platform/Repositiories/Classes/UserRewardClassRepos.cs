using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class UserRewardClassRepos : IUserRewardRepository
    {
        private readonly DbMain_CFS _CFS;

        public UserRewardClassRepos(DbMain_CFS dbMain_CFS)
        {
            _CFS = dbMain_CFS;
        }

        public async Task<List<Reward>> GetEarnedRewardsAsync(int userId)
        {
            var earnedRewards = await _CFS.Rewards
                .Include(r => r.UserRewards)  // Load related UserRewards
                .Where(r => r.UserRewards.Any(ur => ur.UserId == userId))  // Filter by current user
                .ToListAsync();

            return earnedRewards;
        }

        //public async Task<List<Reward>> GetEarnedRewardsAsync(int userId)
        //{
        //    var rewardIds = await _CFS.UserRewards
        // .Where(ur => ur.UserId == userId)
        // .Select(ur => ur.RewardId)
        // .ToListAsync();

        //    var earnedRewards = await _CFS.Rewards
        //        .Include(r => r.UserRewards)  // Include UserRewards
        //        .Where(r => rewardIds.Contains(r.RewardId))
        //        .ToListAsync();

        //    return earnedRewards;
        //}
    }
}
