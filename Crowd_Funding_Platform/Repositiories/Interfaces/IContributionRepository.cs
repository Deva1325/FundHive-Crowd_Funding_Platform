using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IContributionRepository
    {
        Task<string> CreateOrderAsync(decimal amount, int campaignId, int contributorId);
        Task<bool> VerifyPaymentAsync(string orderId, string paymentId, string signature);
        //Task<bool> SaveContributionAsync(Contribution contribution);

         Task<bool> SaveContributionAsync(string orderId, string paymentId);
        Task AssignRewardAsync(int userId);

    }
}
