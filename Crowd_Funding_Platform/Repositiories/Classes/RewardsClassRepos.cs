using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class RewardsClassRepos : IRewards
    {
        private readonly DbMain_CFS _CFS;

        public RewardsClassRepos(DbMain_CFS dbMain_CFS)
        {
            _CFS = dbMain_CFS;
        }

        public async Task<List<Reward>> GetAllRewards()
        {
            return await _CFS.Rewards.ToListAsync();
        }

        public async Task<Reward> GetRewardById(int rewardId)
        {
            return await _CFS.Rewards.FindAsync(rewardId);
        }
        public async Task<bool> SaveReward(Reward reward)
        {
            try
            {
                if (reward == null) return false;

                if (reward.RewardId== 0)
                {
                    await _CFS.Rewards.AddAsync(reward); // Add new
                }
                else
                {
                    var existingReward = await _CFS.Rewards.FindAsync(reward.RewardId);

                    if (existingReward != null)
                    {
                        existingReward.RewardBatch = reward.RewardBatch;
                        existingReward.RequiredAmount = reward.RequiredAmount;
                        existingReward.RewardDescription = reward.RewardDescription;

                        _CFS.Rewards.Update(existingReward);
                    }
                    else
                    {
                        return false;  // Return false if no reward found to update
                    }
                }

                await _CFS.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteReward(int rewardId)
        {
            try
            {
                var reward = await _CFS.Rewards.FindAsync(rewardId);
                if (reward == null) return false;

                _CFS.Rewards.Remove(reward);
                await _CFS.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
