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
using DocumentFormat.OpenXml.Office2010.Excel;
using ClosedXML.Excel;
using Crowd_Funding_Platform.Repositiories.Classes;
using QuestPDF.Fluent;

namespace Crowd_Funding_Platform.Controllers
{
    public class ContributionsController : BaseController
    {
        private readonly IUser _user;
        private readonly DbMain_CFS _context;
        private readonly IConfiguration _config;
        private readonly IActivityRepository _activityRepository;
        private readonly string _razorpayKey;
        private readonly string _razorpaySecret;
        private readonly IEmailSenderRepos _emailSender;      
        private readonly IContributionRepository _contributionRepository;      
        public ContributionsController(DbMain_CFS cFS, IConfiguration config,IUser user, IEmailSenderRepos emailSender, IActivityRepository activityRepository, IContributionRepository contributionRepository,ISidebarRepos sidebar): base(sidebar)
        {
            _context = cFS;
            _config = config;
            _razorpayKey = _config["Razorpay:Key"];
            _razorpaySecret = _config["Razorpay:Secret"];
            _emailSender = emailSender;
            _contributionRepository = contributionRepository;
            _user=user;
            _activityRepository = activityRepository;
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

                    if ((campaign.Status == "Ongoing" || campaign.Status == "Completed") &&
                        campaign.RaisedAmount >= campaign.Requirement)
                    {
                        bool isAlreadyAwarded = _context.CampaignAchievements
                            .Any(x => x.CampaignId == campaign.CampaignId);

                        if (!isAlreadyAwarded)
                        {
                            // Generate Certificate for Creator
                            var creator = await _context.Users.FindAsync(campaign.CreatorId);
                            string certPath = CertificateHelper.GenerateCreatorCertificatePDF(creator);

                            _context.CampaignAchievements.Add(new CampaignAchievement
                            {
                                CampaignId = campaign.CampaignId,
                                CreatorId = campaign.CreatorId,
                                IsGoalAchieved = true,
                                CertificatePath = certPath,
                                AwardDate = DateTime.Now
                            });

                            await _context.SaveChangesAsync();

                            // Send Email with Certificate
                            await _emailSender.SendEmailAsync(
                                toEmail: creator.Email,
                                userName: creator.Username,
                                subject: "🎉 You're Now a Verified Campaign Creator!",
                                body: "Verified Campaign Creator | CERTIFICATE_PATH:" + certPath,
                                emailType: "CreatorCertificate"
                            );
                        }
                    }
                }


                await _context.SaveChangesAsync();

                // ✅ Add activity log
                string desc = $"User with ID {userId.Value} contributed ₹{data.amount} to campaign '{campaign?.Title}' (Txn: {contribution.TransactionId}).";
                _activityRepository.AddNewActivity(
                    userId: userId.Value,
                    activityType: "Contribution",
                    description: desc,
                    tableName: "Contributions",
                    recordId: contribution.ContributionId
                );

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
                Campaign:</strong> {campaign.Title}<br>
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
        public async Task<IActionResult> ContributionsList(string searchString, DateTime? ConDate, int? page)
        {
            var contributors = await _user.GetAllContributions_Contributor();

            //// Search filter
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    contributors = contributors
            //        .Where(c => c.Contributor?.Username != null &&
            //                    c.Contributor.Username.ToLower().Contains(searchString.ToLower()))
            //        .ToList();
            //}

            // Search filter for name or email
            if (!string.IsNullOrEmpty(searchString))
            {
                contributors = contributors
                    .Where(c =>
                        (c.Contributor?.Username?.ToLower().Contains(searchString.ToLower()) ?? false) ||
                        (c.Contributor?.Email?.ToLower().Contains(searchString.ToLower()) ?? false))
                    .ToList();
            }

            if (ConDate.HasValue)
            {
                contributors = contributors.Where(c => c.Date >= ConDate.Value).ToList();
            }
           

            // Default page = 1, pageSize = 10
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var pagedContributors = contributors.ToPagedList(pageNumber, pageSize);

            ViewBag.CurrentFilter = searchString;
            ViewBag.DateFilter = ConDate;

            return View(pagedContributors);
        }

        [HttpGet]
        public async Task<IActionResult> ExportContributorsListToExcel(string searchString, int? campaignId, string paymentStatus)
        {
            var contributions = await _user.GetAllContributions_Contributor();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.ToLower();
                contributions = contributions.Where(c =>
                    (c.Campaign?.Title?.ToLower().Contains(searchString) ?? false) ||
                    (c.Contributor?.Username?.ToLower().Contains(searchString) ?? false) ||
                    c.PaymentStatus.ToLower().Contains(searchString) ||
                    c.Amount.ToString().Contains(searchString)).ToList();
            }

            if (campaignId.HasValue)
            {
                contributions = contributions.Where(c => c.CampaignId == campaignId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                contributions = contributions.Where(c => c.PaymentStatus.Equals(paymentStatus, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.CurrentFilter = searchString;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Contributors");

            // Header
            worksheet.Cell(1, 1).Value = "Sr No";
            worksheet.Cell(1, 2).Value = "Contributor Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Total Contributions";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Date";

            // Data rows
            int row = 2;
            int srNo = 1;
            foreach (var c in contributions)
            {
                worksheet.Cell(row, 1).Value = srNo++;
                worksheet.Cell(row, 2).Value = c.Contributor.Username;
                worksheet.Cell(row, 3).Value = c.Contributor.Email;
                worksheet.Cell(row, 4).Value = c.Amount;
                worksheet.Cell(row, 5).Value = c.PaymentStatus;
                worksheet.Cell(row, 6).Value = c.Date?.ToString("dd MMM yyyy") ?? "-";
                row++;
            }

            // Auto adjust columns
            worksheet.Columns().AdjustToContents();

            // Return Excel file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Contributors_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportContributionsToPdf(string searchString, DateTime? ConDate)
        {
            try
            {
                var contributors = await _user.GetAllContributions_Contributor();

                // Apply filters
                if (!string.IsNullOrEmpty(searchString))
                {
                    contributors = contributors
                        .Where(c =>
                            (c.Contributor?.Username?.ToLower().Contains(searchString.ToLower()) ?? false) ||
                            (c.Contributor?.Email?.ToLower().Contains(searchString.ToLower()) ?? false))
                        .ToList();
                }

                if (ConDate.HasValue)
                {
                    contributors = contributors.Where(c => c.Date >= ConDate.Value).ToList();
                }

                if (contributors == null || !contributors.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No contributions available to generate the report."
                    });
                }

                // Generate PDF
                var document = new ContributionsList_PDF(contributors);
                var pdfBytes = document.GeneratePdf();

                return File(pdfBytes, "application/pdf", "ContributionsReport.pdf");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while generating the contributions report PDF.",
                    error = ex.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> EachConributionsRecord(int contributorId, string searchString, int? page)
        {
            var contributions = await _contributionRepository.GetContributionsByContributorId(contributorId);

            // Apply basic filtering (if needed)
            if (!string.IsNullOrEmpty(searchString))
            {
                contributions = contributions
                    .Where(c => c.Campaign?.Title?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var pagedList = contributions.ToPagedList(pageNumber, pageSize);

            ViewBag.ContributorId = contributorId;
            ViewBag.SearchTerm = searchString;

            return View(pagedList);
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