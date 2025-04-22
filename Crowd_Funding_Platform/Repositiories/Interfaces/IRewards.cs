using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IRewards
    {
        Task<List<Reward>> GetAllRewards();
        Task<Reward> GetRewardById(int rewardId);
        Task<bool> SaveReward(Reward reward, IFormFile? ImageFile);
        Task<bool> DeleteReward(int rewardId);

        //Task CheckAndAssignRewardAsync(int userId);
        //Task<Reward> GetUserRewardAsync(int userId);
        //Task<byte[]> GenerateCertificateAsync(int userId, string rewardBatch);
        //Task SendCertificateByEmailAsync(int userId, string rewardBatch);
    }
}
