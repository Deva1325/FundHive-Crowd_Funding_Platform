using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using Crowd_Funding_Platform.Helpers;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Humanizer;
using Microsoft.AspNetCore.Components.Web;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class ContributionReposClass : IContributionRepository
    {
        private readonly DbMain_CFS _CFS; 
        private readonly IConfiguration _configuration;
        private readonly IEmailSenderRepos _emailService;

        public ContributionReposClass(DbMain_CFS dbMain_CFS,IConfiguration configuration, IEmailSenderRepos emailService)
        {
            _CFS = dbMain_CFS;
            _configuration = configuration; 
            _emailService = emailService;
        }

        public async Task<string> CreateOrderAsync(decimal amount, int campaignId, int contributorId)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(
                    _configuration["Razorpay:Key"],
                    _configuration["Razorpay:Secret"]);

                Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) }, // Razorpay amount in paise
                { "currency", "INR" },
                { "payment_capture", 1 }
            };

                Order order = client.Order.Create(options);

                // Save contribution order in DB
                Contribution contribution = new Contribution
                {
                    Amount = amount,
                    CampaignId = campaignId,
                    ContributorId = contributorId,
                    OrderId = order["id"].ToString(),
                    PaymentStatus = "Pending",
                    Date = DateTime.Now
                };

                await _CFS.Contributions.AddAsync(contribution);
                await _CFS.SaveChangesAsync();

                return order["id"].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateOrderAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> VerifyPaymentAsync(string orderId, string paymentId, string signature)
        {
            try
            {
                string key = _configuration["rzp_test_lP7c4T9H5YGiXt"];
                string secret = _configuration["FGjFH8q4GM1Ogr12F0IdYZfj"];

                string generatedSignature = Crowd_Funding_Platform.Helpers.Utils.CalculateHMAC_SHA256(orderId + "|" + paymentId, secret);

                return generatedSignature == signature;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VerifyPaymentAsync: {ex.Message}");
                return false;
            }
        }

        //public async Task<bool> SaveContributionAsync(Contribution contribution)
        //{
        //    try
        //    {
        //        await _CFS.Contributions.AddAsync(contribution);
        //        await _CFS.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in SaveContributionAsync: {ex.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> SaveContributionAsync(string orderId, string paymentId)
        {
            var contribution = await _CFS.Contributions.FirstOrDefaultAsync(c => c.OrderId == orderId);
            if (contribution == null)
                return false;

            contribution.PaymentId = paymentId;
            contribution.PaymentStatus = "Success";
            await _CFS.SaveChangesAsync();
            return true;
        }

        public async Task AssignRewardAsync(int userId)
        {
            var totalContribution = await _CFS.Contributions
                .Where(c => c.ContributorId == userId && c.PaymentStatus == "Success")
                .SumAsync(c => c.Amount);

            var allRewards = await _CFS.Rewards.OrderBy(r => r.RequiredAmount).ToListAsync();
            var userRewardIds = await _CFS.UserRewards
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RewardId)
                .ToListAsync();

            // var user = await _CFS.Users.FindAsync(userId);

            // New - better way:
            var user = await _CFS.Contributions
                .Include(c => c.Contributor)
                .Where(c => c.ContributorId == userId && c.PaymentStatus == "Success")
                .Select(c => c.Contributor)
                .FirstOrDefaultAsync();

            foreach (var reward in allRewards)
            {
                if (totalContribution >= reward.RequiredAmount && !userRewardIds.Contains(reward.RewardId))
                {
                    UserReward userReward = new UserReward
                    {
                        UserId = userId,
                        RewardId = reward.RewardId,
                        TotalContribution = totalContribution,
                        Timestamp = DateTime.Now,
                        IsCertificateGenerated = true
                    };

                    await _CFS.UserRewards.AddAsync(userReward);
                    await _CFS.SaveChangesAsync();

                    string pdfPath = CertificateHelper.GenerateCertificatePDF(user, reward.RewardBatch, totalContribution);
                    string badgeName = "Gold Badge";

                    await _emailService.SendEmailAsync(
                        toEmail: user.Email,
                        userName: user.Username,
                        subject: "🎉 Your Gold Badge Certificate!",
                        body: $"{badgeName}|CERTIFICATE_PATH:{pdfPath}",
                        emailType: "Certificate"
                    );


                    //                    // ✅ Generate and email certificate
                    //                    string certPath = CertificateHelper.GenerateCertificatePDF(user, reward.RewardBatch, totalContribution);
                    //                    await _emailService.SendEmailAsync(
                    //    toEmail: user.Email,
                    //    userName: user.Username,
                    //    subject: "🎉 Your Gold Badge Certificate!",
                    //    body: "CERTIFICATE_PATH:/path/to/generated_certificate.pdf",
                    //    emailType: "Certificate"
                    //);
                }
            }
        }

        public async Task<List<Contribution>> GetContributionsByContributorId(int contributorId)
        {
            var contributions = await (
                from c in _CFS.Contributions
                join cam in _CFS.Campaigns on c.CampaignId equals cam.CampaignId
                join u in _CFS.Users on c.ContributorId equals u.UserId
                where c.ContributorId == contributorId
                orderby c.Date descending
                select new Contribution
                {
                    ContributionId = c.ContributionId,
                    Amount = c.Amount,
                    Date = c.Date,
                    PaymentStatus = c.PaymentStatus,
                    TransactionId = c.TransactionId,
                    PaymentId = c.PaymentId,
                    OrderId = c.OrderId,
                    Campaign = new Campaign
                    {
                        CampaignId = cam.CampaignId,
                        Title = cam.Title
                    },
                    Contributor = new User
                    {
                        UserId = u.UserId,
                        ProfilePicture = u.ProfilePicture,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email
                    }
                }).ToListAsync();

            return contributions;
        }

    }
}
