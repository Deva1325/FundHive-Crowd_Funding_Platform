using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

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
            var creators = await (
                    from Creators in _CFS.CreatorApplications
                    join Users in _CFS.Users on Creators.UserId equals Users.UserId
                    where Creators.Status == "Pending"
                    select new CreatorApplication
                    {
                        ApplicationId = Creators.ApplicationId,
                        DocumentType = Creators.DocumentType,
                        DocumentPath = Creators.DocumentPath,
                        Status = Creators.Status,
                        SubmissionDate = Creators.SubmissionDate,
                        StatusUpdatedDate = Creators.StatusUpdatedDate,
                        AdminRemarks = Creators.AdminRemarks,
                        User = new User
                        {
                            UserId = Users.UserId,
                            Username = Users.Username
                        }
                    }
                ).OrderByDescending(c => c.SubmissionDate).ToListAsync();

            return creators;
        }

        public async Task<List<Campaign>> GetAllCampaigns()
        {
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

            string newFilePath = null; // Store new image path if uploaded

            if (MediaUrl != null && MediaUrl.Length > 0)
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

                newFilePath = "/Campaign_Media/" + uniqueFileName;
            }

            if (campaign.CampaignId == 0)
            {
                // Add new campaign
                campaign.CreatorId = userId; // Assign user ID

                // Assign image path if uploaded, otherwise keep MediaUrl null
                campaign.MediaUrl = newFilePath;

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

                // If a new image is uploaded, delete the old one
                if (!string.IsNullOrEmpty(newFilePath) && !string.IsNullOrEmpty(existingCampaign.MediaUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCampaign.MediaUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                existingCampaign.Title = campaign.Title;
                existingCampaign.Description = campaign.Description;
                existingCampaign.Requirement = campaign.Requirement;
                existingCampaign.StartDate = campaign.StartDate;
                existingCampaign.EndDate = campaign.EndDate;
                existingCampaign.CategoryId = campaign.CategoryId;

                // Keep existing image if no new image is uploaded
                existingCampaign.MediaUrl = newFilePath ?? existingCampaign.MediaUrl;
            }

            await _CFS.SaveChangesAsync();
            return (true, "Campaign saved successfully.");
        }


        public async Task<Campaign?> GetCampaignById(int id)
        {
            return await _CFS.Campaigns.FirstOrDefaultAsync(c => c.CampaignId == id);
        }

        public async Task<bool> DeleteCampaign(int id)
        {
            var campaignId = await _CFS.Campaigns.FindAsync(id);

            if (campaignId == null)
            {
                return false;
            }

            _CFS.Campaigns.Remove(campaignId);
            await _CFS.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message)> ApproveCreator(int id)
        {
            var creator = await _CFS.CreatorApplications.FindAsync(id);
            if (creator == null) return (false, "Creator application not found.");

            var user = await _CFS.Users.FindAsync(creator.UserId);
            if (user == null) return (false, "User not found.");

            // Update Creator Application status and timestamp
            creator.Status = "Approved";
            creator.StatusUpdatedDate = DateTime.UtcNow; // Set the current timestamp

            // Update User table to mark as approved creator
            user.IsCreatorApproved = true;

            await _CFS.SaveChangesAsync();
            return (true, "Creator approved successfully.");
        }


        public async Task<(bool success, string message)> RejectCreator(int id)
        {
            var creator = await _CFS.CreatorApplications.FindAsync(id);
            if (creator == null) return (false, "Creator application not found.");

            creator.Status = "Rejected";
            await _CFS.SaveChangesAsync();
            return (true, "Creator rejected successfully.");
        }

        public async Task<CreatorApplication?> GetApplicationById(int id)
        {
            return await _CFS.CreatorApplications.FirstOrDefaultAsync(c => c.ApplicationId == id);
        }
    }
}
