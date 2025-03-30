using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class RewardsController : BaseController
    {
        private readonly IRewards _rewards;
        private readonly DbMain_CFS _dbMain;

        public RewardsController(DbMain_CFS dbMain,IRewards rewards,ISidebarRepos sidebar) : base(sidebar)
        {
            _dbMain = dbMain;
            _rewards = rewards;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RewardsList()
        {
            var rewards = await _rewards.GetAllRewards();
            return View(rewards);
        }

        [HttpGet]
        public async Task<IActionResult> SaveRewards(int? id)
        {
            if (id == null || id == 0)
                return View(new Reward()); // Add Mode (Empty form)

            var reward = await _rewards.GetRewardById(id.Value);
            if (reward == null)
                return NotFound();

            return View(reward); // Edit Mode (Prefilled form)
        }

        [HttpPost]
        public async Task<IActionResult> SaveRewards(Reward reward)
        {
            try
            {
                bool isNew = reward.RewardId == 0;  // Check if it's a new category

                // ✅ Ensure ID is properly passed and checked
                bool isSaved = await _rewards.SaveReward(reward);

                if (isSaved)
                {
                    return Json(new
                    {
                        success = true,
                        message = isNew ? "Reward added successfully!" : "Reward updated successfully!"
                    });
                }
                return Json(new { success = false, message = "Failed to save reward!" });
            }
            catch (Exception)
            {

                throw;
            }             
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _rewards.DeleteReward(id);
            return Json(new
            {
                success = result,
                message = result ? "Reward deleted successfully!" : "Failed to delete Reward."
            });
        }
    }
}
