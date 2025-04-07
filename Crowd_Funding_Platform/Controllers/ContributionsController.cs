using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;

namespace Crowd_Funding_Platform.Controllers
{
    public class ContributionsController : Controller
    {
        private readonly DbMain_CFS _context;
        private readonly IConfiguration _config;
        private readonly string _razorpayKey;
        private readonly string _razorpaySecret;
        private readonly IEmailSender _emailSender;
        
        public ContributionsController(DbMain_CFS cFS, IConfiguration config,IEmailSender emailSender)
        {
            _context = cFS;
            _config = config;
            _razorpayKey = _config["Razorpay:Key"];
            _razorpaySecret = _config["Razorpay:Secret"];
            _emailSender = emailSender; 
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] RazorOrderRequest req)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(_razorpayKey, _razorpaySecret);

                Dictionary<string, object> options = new Dictionary<string, object>
                {
                    { "amount", req.Amount * 100 }, // Razorpay uses paise
                    { "currency", "INR" },
                    { "receipt", Guid.NewGuid().ToString() }
                };

                Order order = client.Order.Create(options);

                return Json(new
                {
                    id = order["id"].ToString(),
                    amount = order["amount"],
                    currency = order["currency"]
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error creating order: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAndStorePayment([FromBody] RazorVerifyRequest data)
        {
            try
            {
                // Step 1: Signature Verification
                string payload = data.razorpay_order_id + "|" + data.razorpay_payment_id;
                string generatedSignature;

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_razorpaySecret)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }

                if (generatedSignature != data.razorpay_signature)
                {
                    return Json(new { success = false, message = "Signature verification failed!" });
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not logged in!" });
                }

                // Save Contribution
                var contribution = new Contribution
                {
                    ContributorId = userId.Value,
                    CampaignId = data.campaignId,
                    Amount = data.amount,
                    Date = DateTime.Now,
                    PaymentStatus = "Pending"
                };

                _context.Contributions.Add(contribution);
                await _context.SaveChangesAsync();

                string transactionId = "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss") + contribution.ContributionId;
                contribution.TransactionId = transactionId;
                _context.Contributions.Update(contribution);
                await _context.SaveChangesAsync();

                // Fetch Razorpay payment details
                RazorpayClient client = new RazorpayClient(_razorpayKey, _razorpaySecret);
                Payment payment = client.Payment.Fetch(data.razorpay_payment_id);

                var paymentDetails = new PaymentDetail
                {
                    ContributionId = contribution.ContributionId,
                    PaymentMethod = payment["method"]?.ToString(),
                    RazorpayPaymentId = data.razorpay_payment_id,
                    OrderId = data.razorpay_order_id,
                    Signature = data.razorpay_signature,
                    PaymentDate = DateTime.Now,
                    Status = "Success"
                };

                _context.PaymentDetails.Add(paymentDetails);
                await _context.SaveChangesAsync();

                // Update Contribution
                contribution.PaymentStatus = "Success";
                contribution.Status = "Confirmed";
                _context.Contributions.Update(contribution);

                // ✅ Step: Auto-Update Campaign Raised Amount
                var campaign = await _context.Campaigns.FindAsync(data.campaignId);
                if (campaign != null)
                {
                    campaign.RaisedAmount += data.amount;
                    _context.Campaigns.Update(campaign);
                }

                await _context.SaveChangesAsync();

                // ✅ Step: Send Thank You Email with PDF Receipt

                var user = await _context.Users.FindAsync(userId.Value);
                var receiptPdf = PdfHelper.GenerateReceipt(user.Username, campaign.Title, data.amount, transactionId);
                //EmailHelper.SendThankYouEmail(user.Email, user.Username, campaign.Title, data.amount, receiptPdf);
                await _emailSender.SendEmailAsync(
                user.Email,
                user.Username,
                "Thank You for Your Contribution!",
                receiptPdf, // This is the HTML string of the PDF
                "ThankYou");

                return Json(new { success = true, message = "Payment verified & receipt emailed!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving payment: " + ex.Message });
            }
        }

    }
}

