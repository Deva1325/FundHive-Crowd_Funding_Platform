using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Crowd_Funding_Platform.Controllers
{
    public class UserRewardController : Controller
    {
        private readonly IUserRewardRepository _userReward;
        private readonly DbMain_CFS _dbMain;

        public UserRewardController(DbMain_CFS dbMain, IUserRewardRepository rewards)
        {
            _dbMain = dbMain;
            _userReward = rewards;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MyRewards()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            var earnedRewards = await _userReward.GetEarnedRewardsAsync(userId.Value);
            return View(earnedRewards);
        }

    }
}
