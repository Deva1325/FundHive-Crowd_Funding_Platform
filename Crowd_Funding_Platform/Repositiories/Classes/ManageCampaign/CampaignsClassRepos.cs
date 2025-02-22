using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes.ManageCampaign
{
    public class CampaignsClassRepos : ICampaignsRepos
    {
        private readonly DbMain_CFS _CFS;

        public CampaignsClassRepos(DbMain_CFS CFS)
        {
            _CFS = CFS;
        }
        /// <summary>
        /// Fetches all campaigns with "Pending" status.
        /// </summary>
        public async Task<List<CreatorApplication>> GetPendingCampaigns()
        {
            return await _CFS.CreatorApplications
                .Where(c => c.Status == "Pending").ToListAsync();
        }

        public async Task<List<Campaign>> GetAllCampaigns() {
            return await _CFS.Campaigns.OrderByDescending(c => c.StartDate).ToListAsync();
        }

        /// <summary>
        /// Saves a campaign after verifying user eligibility.
        /// </summary>
        public async Task<(bool success, string message)> SaveCampaigns(Campaign campaign, int userId, IFormFile? MediaUrl)
        {
            var user = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            if (!(user.IsCreatorApproved ?? false))
            {
                var creatorApplication = await _CFS.CreatorApplications.FirstOrDefaultAsync(ca => ca.UserId == userId);
                if (creatorApplication == null)
                {
                    return (false, "Please fill out the document form to request campaign creation.");
                }

                if (creatorApplication.Status == "Pending")
                {
                    return (false, "Your request is pending. Please wait for admin approval.");
                }
            }

            if (MediaUrl != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Campaign_Media");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(MediaUrl.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await MediaUrl.CopyToAsync(fileStream);
                }

                campaign.MediaUrl = "/media/" + uniqueFileName;
            }

            if (campaign.CampaignId == 0)
            {
                // Add new campaign
                campaign.CreatorId = userId; // Assign user ID

                // Get today's date as DateOnly
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);

                if (campaign.StartDate > today)
                {
                    campaign.Status = "Upcoming";
                }
                else if (campaign.StartDate <= today && campaign.EndDate >= today)
                {
                    campaign.Status = "Ongoing";
                }
                else if (campaign.EndDate < today)
                {
                    campaign.Status = "Completed";
                }
                _CFS.Campaigns.Add(campaign);
            }
            else
            {
                // Edit existing campaign
                var existingCampaign = await _CFS.Campaigns.FindAsync(campaign.CampaignId);
                if (existingCampaign == null)
                {
                    return (false, "Campaign not found.");
                }

                existingCampaign.Title = campaign.Title;
                existingCampaign.Description = campaign.Description;
                existingCampaign.Requirement = campaign.Requirement;
                existingCampaign.StartDate = campaign.StartDate;
                existingCampaign.EndDate = campaign.EndDate;
                existingCampaign.CategoryId = campaign.CategoryId;
                existingCampaign.MediaUrl = campaign.MediaUrl;
            }

            await _CFS.SaveChangesAsync();
            return (true, "Campaign saved successfully.");
        }

    
}
}
