using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Helpers;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
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
using Crowd_Funding_Platform.Repositiories.Interfaces;
using X.PagedList.Extensions;

namespace Crowd_Funding_Platform.Controllers
{
    public class ContributionsController : BaseController
    {
        private readonly IUser _user;
        private readonly DbMain_CFS _context;
        private readonly IConfiguration _config;
        private readonly string _razorpayKey;
        private readonly string _razorpaySecret;
        private readonly IEmailSenderRepos _emailSender;      
        private readonly IContributionRepository _contributionRepository;      
        public ContributionsController(DbMain_CFS cFS, IConfiguration config,IUser user, IEmailSenderRepos emailSender, IContributionRepository contributionRepository,ISidebarRepos sidebar): base(sidebar)
        {
            _context = cFS;
            _config = config;
            _razorpayKey = _config["Razorpay:Key"];
            _razorpaySecret = _config["Razorpay:Secret"];
            _emailSender = emailSender;
            _contributionRepository = contributionRepository;
            _user=user;
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
                //string generatedSignature;

                //using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_razorpaySecret)))
                //{
                //    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                //    generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                //}

                //if (generatedSignature != data.razorpay_signature)
                //{
                //    return Json(new { success = false, message = "Signature verification failed!" });
                //}

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
                contribution.PaymentId=data.razorpay_payment_id;
                contribution.OrderId = data.razorpay_order_id;
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

                // Convert PDF to Base64
                string base64Pdf = PdfHelper.GenerateReceiptBase64(
                    user.Username,
                    campaign.Title,
                    data.amount,
                    contribution.TransactionId
                );

                string emailBodyWithPdf = $@"
                <p>Dear {user.Username},</p>
                <p>Thank you for your contribution!</p>
                <p><strong                   <strong>Transaction ID:</strong> {contribution.TransactionId}</p>
>Campaign:</strong> {campaign.Title}<br>
                   <strong>Amount:</strong> ₹{data.amount}<br>
                <p>We’ve also attached your official receipt (PDF) for your records.</p>
                <!-- PDF-ATTACHMENT:{base64Pdf} -->";
              
               await _emailSender.SendEmailAsync(user.Email, user.Username, "Thank You for Your Contribution!", emailBodyWithPdf, "Contribution");

                await _contributionRepository.AssignRewardAsync(userId.Value);

                return Json(new { success = true, message = "Payment verified & receipt emailed!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving payment: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ContributionsList(string searchString, string category, int? page)
        {
            var contributors = await _user.GetAllContributorsAsync();

            // Get unique categories
            var categories = contributors
                             .Where(c => c.Campaign?.Category != null && !string.IsNullOrEmpty(c.Campaign.Category.Name))
                             .Select(c => c.Campaign.Category.Name)
                             .Distinct()
                             .ToList();

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                contributors = contributors
                    .Where(c => c.Contributor?.Username != null &&
                                c.Contributor.Username.ToLower().Contains(searchString.ToLower()))
                    .ToList();
            }

            // Category filter
            if (!string.IsNullOrEmpty(category))
            {
                contributors = contributors
                    .Where(c => c.Campaign?.Category?.Name == category)
                    .ToList();
            }

            // Default page = 1, pageSize = 10
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var pagedContributors = contributors.ToPagedList(pageNumber, pageSize);

            ViewBag.CurrentFilter = searchString;
            ViewBag.SelectedCategory = category;
            ViewBag.Categories = categories;

            return View(pagedContributors);
        }


        //[HttpGet]
        //public async Task<IActionResult> MyContributions(string searchString, string category, int? page)
        //{
        //    var contributors = await _user.GetAllContributorsAsync();

        //    // Get unique categories
        //    var categories = contributors
        //                     .Where(c => c.Campaign?.Category != null && !string.IsNullOrEmpty(c.Campaign.Category.Name))
        //                     .Select(c => c.Campaign.Category.Name)
        //                     .Distinct()
        //                     .ToList();

        //    // Search filter
        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        contributors = contributors
        //            .Where(c => c.Contributor?.Username != null &&
        //                        c.Contributor.Username.ToLower().Contains(searchString.ToLower()))
        //            .ToList();
        //    }

        //    // Category filter
        //    if (!string.IsNullOrEmpty(category))
        //    {
        //        contributors = contributors
        //            .Where(c => c.Campaign?.Category?.Name == category)
        //            .ToList();
        //    }

        //    // Default page = 1, pageSize = 10
        //    int pageSize = 5;
        //    int pageNumber = page ?? 1;

        //    var pagedContributors = contributors.ToPagedList(pageNumber, pageSize);

        //    ViewBag.CurrentFilter = searchString;
        //    ViewBag.SelectedCategory = category;
        //    ViewBag.Categories = categories;

        //    return View(pagedContributors);
        //}

    }
}

