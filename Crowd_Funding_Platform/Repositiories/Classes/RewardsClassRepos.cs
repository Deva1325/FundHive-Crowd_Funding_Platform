using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class RewardsClassRepos : IRewards
    {
        private readonly DbMain_CFS _CFS;
        //private readonly IWebHostEnvironment _env;
        //private readonly IEmailSenderRepos _emailService;

        public RewardsClassRepos(DbMain_CFS dbMain_CFS)
        {
            _CFS = dbMain_CFS;
            //_env = env;
            //_emailService = emailService;
        }

        public async Task<List<Reward>> GetAllRewards()
        {
            return await _CFS.Rewards.ToListAsync();
        }
        public async Task<Reward> GetRewardById(int rewardId)
        {
            return await _CFS.Rewards.FindAsync(rewardId);
        }
        public async Task<bool> SaveReward(Reward reward, IFormFile? ImageFile)
        {
            try
            {
                if (reward == null) return false;

                string newFilePath = null;
                // Handle main image (Thumbnail image upload)
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Reward_Badges");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    newFilePath = "/Reward_Badges/" + uniqueFileName;

                }

                if (reward.RewardId== 0)
                {
                    await _CFS.Rewards.AddAsync(reward); // Add new
                }
                else
                {
                    var existingReward = await _CFS.Rewards.FindAsync(reward.RewardId);

                    // Delete old main image if new one is uploaded
                    if (!string.IsNullOrEmpty(newFilePath) && !string.IsNullOrEmpty(existingReward.BadgeIcon))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingReward.BadgeIcon.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    if (existingReward != null)
                    {
                        existingReward.RewardBatch = reward.RewardBatch;
                        existingReward.RequiredAmount = reward.RequiredAmount;
                        existingReward.RewardDescription = reward.RewardDescription;
                        //existingReward.BadgeIcon = reward.BadgeIcon;
                        existingReward.BadgeIcon = newFilePath ?? existingReward.BadgeIcon;
                        
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

        //public async Task CheckAndAssignRewardAsync(int userId)
        //{
        //    var totalContribution = await _CFS.Contributions
        //        .Where(c => c.ContributorId == userId && c.PaymentStatus == "Success")
        //        .SumAsync(c => c.Amount);

        //    var eligibleReward = await _CFS.Rewards
        //        .Where(r => totalContribution >= r.RequiredAmount)
        //        .OrderByDescending(r => r.RequiredAmount)
        //        .FirstOrDefaultAsync();

        //    if (eligibleReward == null) return;

        //    var existingReward = await _CFS.UserRewards
        //        .Where(ur => ur.UserId == userId)
        //        .OrderByDescending(ur => ur.TotalContribution)
        //        .FirstOrDefaultAsync();

        //    if (existingReward == null || eligibleReward.RewardId != existingReward.RewardId)
        //    {
        //        var userReward = new UserReward
        //        {
        //            UserId = userId,
        //            RewardId = eligibleReward.RewardId,
        //            TotalContribution = totalContribution,
        //            Timestamp = DateTime.UtcNow
        //        };

        //        _CFS.UserRewards.Add(userReward);
        //        await _CFS.SaveChangesAsync();

        //        // Generate certificate
        //      //  await _certificateService.GenerateAndSendCertificateAsync(userId, eligibleReward.RewardBatch, totalContribution);
        //    }
        //}

        //public async Task<Reward> GetUserRewardAsync(int userId)
        //{
        //    return await _CFS.UserRewards
        //        .Where(ur => ur.UserId == userId)
        //        .OrderByDescending(ur => ur.TotalContribution)
        //        .Select(ur => ur.Reward)
        //        .FirstOrDefaultAsync();
        //}
    }


    //public async Task<byte[]> GenerateCertificateAsync(int userId, string rewardBatch)
    //{
    //    var user = await _CFS.Users.FindAsync(userId);
    //    var pdfGen = new PdfCertificateGenerator();
    //    var contribution = await _CFS.Contributions
    //        .Where(c => c.ContributorId == userId && c.PaymentStatus == "Success")
    //        .SumAsync(c => (decimal?)c.Amount) ?? 0;

    //    return pdfGen.CreateCertificate(user.Username, rewardBatch, contribution);
    //}

    //public async Task SendCertificateByEmailAsync(int userId, string rewardBatch)
    //{
    //    var user = await _CFS.Users.FindAsync(userId);
    //    var pdf = await GenerateCertificateAsync(userId, rewardBatch);

    //    await _emailService.SendEmailWithAttachmentAsync(user.Email, $"Congratulations on achieving {rewardBatch} badge!",
    //        "Attached is your reward certificate.", pdf, $"{rewardBatch}_Certificate.pdf");
    //}
}

