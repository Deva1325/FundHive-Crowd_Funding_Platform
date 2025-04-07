using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using Crowd_Funding_Platform.Helpers;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class ContributionReposClass
    {
        private readonly DbMain_CFS _CFS; 
        private readonly IConfiguration _configuration;

        public ContributionReposClass(DbMain_CFS dbMain_CFS,IConfiguration configuration)
        {
            _CFS = dbMain_CFS;
            _configuration = configuration; 
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


    }
}
