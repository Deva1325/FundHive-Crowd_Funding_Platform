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


        // Add this method inside the repository class
        private string GetCampaignStatus(DateOnly startDate, DateOnly endDate, DateOnly today)
        {
            if (startDate > today)
            {
                return "Upcoming";
            }
            else if (startDate <= today && endDate >= today)
            {
                return "Ongoing";
            }
            else
            {
                return "Completed";
            }
        }


        //public async Task<(bool success, string message)> SaveCampaigns(Campaign campaign, int userId, IFormFile? ImageFile, List<IFormFile>? GalleryImages)
        //{
        //    var user = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        //    if (user == null)
        //    {
        //        return (false, "User not found.");
        //    }

        //    if (!(user.IsCreatorApproved ?? false))
        //    {
        //        var creatorApplication = await _CFS.CreatorApplications.FirstOrDefaultAsync(ca => ca.UserId == userId);
        //        if (creatorApplication == null)
        //        {
        //            return (false, "Please fill out the document form to request campaign creation.");
        //        }
        //        if (creatorApplication.Status == "Pending")
        //        {
        //            return (false, "Your request is pending. Please wait for admin approval.");
        //        }
        //    }

        //    string newFilePath = null; // Store new image path if uploaded
        //    if (ImageFile != null && ImageFile.Length > 0)
        //    {
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Campaign_Media");
        //        if (!Directory.Exists(uploadsFolder))
        //        {
        //            Directory.CreateDirectory(uploadsFolder);
        //        }
        //        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await ImageFile.CopyToAsync(fileStream);
        //        }
        //        newFilePath = "/Campaign_Media/" + uniqueFileName;
        //    }

        //    if (campaign.CampaignId == 0)
        //    {
        //        campaign.CreatorId = userId;
        //        campaign.MediaUrl = newFilePath;
        //        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        //        if (campaign.StartDate > today)
        //        {
        //            campaign.Status = "Upcoming";
        //        }
        //        else if (campaign.StartDate <= today && campaign.EndDate >= today)
        //        {
        //            campaign.Status = "Ongoing";
        //        }
        //        else if (campaign.EndDate < today)
        //        {
        //            campaign.Status = "Completed";
        //        }
        //        _CFS.Campaigns.Add(campaign);
        //    }
        //    else
        //    {
        //        var existingCampaign = await _CFS.Campaigns.FindAsync(campaign.CampaignId);
        //        if (existingCampaign == null)
        //        {
        //            return (false, "Campaign not found.");
        //        }


        //        if (!string.IsNullOrEmpty(newFilePath) && !string.IsNullOrEmpty(existingCampaign.MediaUrl))
        //        {
        //            string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCampaign.MediaUrl.TrimStart('/'));
        //            if (System.IO.File.Exists(oldFilePath))
        //            {
        //                System.IO.File.Delete(oldFilePath);
        //            }
        //        }
        //        existingCampaign.Title = campaign.Title;
        //        existingCampaign.Description = campaign.Description;
        //        existingCampaign.Requirement = campaign.Requirement;
        //        existingCampaign.StartDate = campaign.StartDate;
        //        existingCampaign.EndDate = campaign.EndDate;
        //        existingCampaign.CategoryId = campaign.CategoryId;
        //        existingCampaign.MediaUrl = newFilePath ?? existingCampaign.MediaUrl;
        //    }
        //    await _CFS.SaveChangesAsync();

        //    // Save Gallery Images
        //    if (GalleryImages != null && GalleryImages.Any())
        //    {
        //        string galleryFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Campaign_Gallery");
        //        Directory.CreateDirectory(galleryFolder);

        //        int sortOrder = 1;
        //        bool isThumbnailSet = string.IsNullOrEmpty(campaign.MediaUrl); // true if no main image

        //        foreach (var image in GalleryImages)
        //        {
        //            string uniqueFile = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        //            string filePath = Path.Combine(galleryFolder, uniqueFile);

        //            using (var fs = new FileStream(filePath, FileMode.Create))
        //            {
        //                await image.CopyToAsync(fs);
        //            }

        //            var campaignImage = new CampaignImage
        //            {
        //                CampaignId = campaign.CampaignId,
        //                ImageUrl = "/Campaign_Gallery/" + uniqueFile,
        //                IsThumbnail = isThumbnailSet,
        //                SortOrder = sortOrder++,
        //                UploadedDate = DateTime.Now
        //            };
        //            isThumbnailSet = false; // only first one will be true

        //            _CFS.CampaignImages.Add(campaignImage);
        //        }

        //        await _CFS.SaveChangesAsync();
        //    }

        //    return (true, "Campaign saved successfully.");
        //}




        //Old Code

        public async Task<(bool success, string message)> SaveCampaigns(Campaign campaign,int userId,IFormFile? ImageFile,
        List<IFormFile>? GalleryImages)
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

                string newFilePath = null;

            // Handle main image (Thumbnail image upload)
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Campaign_Media");
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

                newFilePath = "/Campaign_Media/" + uniqueFileName;

            }

            if (campaign.CampaignId == 0)
            {
                // New campaign
                campaign.CreatorId = userId;
                campaign.MediaUrl = newFilePath;

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
                await _CFS.SaveChangesAsync(); // Save now to get CampaignId

                
            }
            else
            {
                // Update existing campaign
                var existingCampaign = await _CFS.Campaigns.FindAsync(campaign.CampaignId);
                if (existingCampaign == null)
                {
                    return (false, "Campaign not found.");
                }

                // Delete old main image if new one is uploaded
                if (!string.IsNullOrEmpty(newFilePath) && !string.IsNullOrEmpty(existingCampaign.MediaUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCampaign.MediaUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Update fields
                existingCampaign.Title = campaign.Title;
                existingCampaign.Description = campaign.Description;
                existingCampaign.Requirement = campaign.Requirement;
                existingCampaign.StartDate = campaign.StartDate;
                existingCampaign.EndDate = campaign.EndDate;
                existingCampaign.CategoryId = campaign.CategoryId;
                existingCampaign.MediaUrl = newFilePath ?? existingCampaign.MediaUrl;

               
                await _CFS.SaveChangesAsync();
            }

            // Fetch back the campaign to ensure we have a fresh instance with the ID (or use campaign.CampaignId directly)
            int newCampaignId = campaign.CampaignId;

            // Save gallery files (images, pdfs, docs)
            if (GalleryImages != null && GalleryImages.Any())
            {
                string galleryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Campaign_Gallery");
                Directory.CreateDirectory(galleryPath);

                foreach (var file in GalleryImages)
                {
                    if (file.Length > 0)
                    {
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        string allowed = ".jpg,.jpeg,.png,.pdf,.docx,.doc";

                        if (!allowed.Contains(ext)) continue;

                        string filename = Guid.NewGuid() + ext;
                        string fullPath = Path.Combine(galleryPath, filename);

                        using var fs = new FileStream(fullPath, FileMode.Create);
                        await file.CopyToAsync(fs);

                        // Save image path to DB
                        CampaignImage img = new CampaignImage
                        {
                            CampaignId = newCampaignId,
                            ImageUrl= "/Campaign_Gallery/" + filename,
                            UploadedDate = DateTime.Now
                        };

                        _CFS.CampaignImages.Add(img);
                        //_CFS.CampaignImages.Add(new CampaignImage
                        //{
                        //    CampaignId = campaign.CampaignId,
                        //    ImageUrl = "/Campaign_Gallery/" + filename,
                        //    UploadedDate = DateTime.Now
                        //});
                    }
                }

                await _CFS.SaveChangesAsync();
            }
               

                return (true, "Campaign saved successfully.");
            }


        /// <summary>
        /// Saves a campaign after verifying user eligibility.
        /// </summary>
        //public async Task<(bool success, string message)> SaveCampaigns(Campaign campaign, int userId, IFormFile? ImageFile)
        //{
        //    var user = await _CFS.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        //    if (user == null)
        //    {
        //        return (false, "User not found.");
        //    }

        //    if (!(user.IsCreatorApproved ?? false))
        //    {
        //        var creatorApplication = await _CFS.CreatorApplications.FirstOrDefaultAsync(ca => ca.UserId == userId);
        //        if (creatorApplication == null)
        //        {
        //            return (false, "Please fill out the document form to request campaign creation.");
        //        }
        //        if (creatorApplication.Status == "Pending")
        //        {
        //            return (false, "Your request is pending. Please wait for admin approval.");
        //        }
        //    }

        //    string newFilePath = null; // Store new image path if uploaded
        //    if (ImageFile != null && ImageFile.Length > 0)
        //    {
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Campaign_Media");
        //        if (!Directory.Exists(uploadsFolder))
        //        {
        //            Directory.CreateDirectory(uploadsFolder);
        //        }
        //        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await ImageFile.CopyToAsync(fileStream);
        //        }
        //        newFilePath = "/Campaign_Media/" + uniqueFileName;
        //    }

        //    if (campaign.CampaignId == 0)
        //    {
        //        campaign.CreatorId = userId;
        //        campaign.MediaUrl = newFilePath;
        //        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        //        if (campaign.StartDate > today)
        //        {
        //            campaign.Status = "Upcoming";
        //        }
        //        else if (campaign.StartDate <= today && campaign.EndDate >= today)
        //        {
        //            campaign.Status = "Ongoing";
        //        }
        //        else if (campaign.EndDate < today)
        //        {
        //            campaign.Status = "Completed";
        //        }
        //        _CFS.Campaigns.Add(campaign);
        //    }
        //    else
        //    {
        //        var existingCampaign = await _CFS.Campaigns.FindAsync(campaign.CampaignId);
        //        if (existingCampaign == null)
        //        {
        //            return (false, "Campaign not found.");
        //        }

        //        //// Validate date only if changed
        //        //if (campaign.StartDate != existingCampaign.StartDate || campaign.EndDate != existingCampaign.EndDate)
        //        //{
        //        //    if (campaign.EndDate < campaign.StartDate)
        //        //    {
        //        //        return (false, "End date cannot be before start date.");
        //        //    }
        //        //}

        //        if (!string.IsNullOrEmpty(newFilePath) && !string.IsNullOrEmpty(existingCampaign.MediaUrl))
        //        {
        //            string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCampaign.MediaUrl.TrimStart('/'));
        //            if (System.IO.File.Exists(oldFilePath))
        //            {
        //                System.IO.File.Delete(oldFilePath);
        //            }
        //        }
        //        existingCampaign.Title = campaign.Title;
        //        existingCampaign.Description = campaign.Description;
        //        existingCampaign.Requirement = campaign.Requirement;
        //        existingCampaign.StartDate = campaign.StartDate;
        //        existingCampaign.EndDate = campaign.EndDate;
        //        existingCampaign.CategoryId = campaign.CategoryId;
        //        existingCampaign.MediaUrl = newFilePath ?? existingCampaign.MediaUrl;
        //    }
        //    await _CFS.SaveChangesAsync();
        //    return (true, "Campaign saved successfully.");
        //}

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


        // Fetch all campaigns with creator username
        public List<Campaign> ShowCampaignCases()
        {
            return _CFS.Campaigns
                .Include(c => c.Creator)  // Include creator details
                .Select(c => new Campaign
                {
                    CampaignId = c.CampaignId,
                    Title = c.Title,
                    Description = c.Description,
                    Requirement = c.Requirement,
                    RaisedAmount = c.RaisedAmount,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MediaUrl = c.MediaUrl,
                    Status = c.Status,
                    CreatorId = c.CreatorId,
                    Creator = new User
                    {
                        UserId = c.CreatorId,
                        FirstName = c.Creator.FirstName // Only include username
                    }
                })
                .ToList();
        }

        // Fetch a specific campaign by ID
        public Campaign DetailCampaignCases(int campaignId)
        {
            return _CFS.Campaigns
                .Include(c => c.Creator)  // Include creator details    
                .Where(c => c.CampaignId == campaignId)
                .Select(c => new Campaign
                {
                    CampaignId = c.CampaignId,
                    Title = c.Title,
                    Description = c.Description,
                    Requirement = c.Requirement,
                    RaisedAmount = c.RaisedAmount,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MediaUrl = c.MediaUrl,
                    Status = c.Status,
                    CreatorId = c.CreatorId,
                    Creator = new User
                    {
                        UserId = c.CreatorId,
                        FirstName = c.Creator.FirstName  // Include only username areyyy bc 
                    }
                })
                .FirstOrDefault();
        }




        public async Task<List<Campaign>> GetCampaignsByCreator(int creatorId)
        {
            return await _CFS.Campaigns
                .Where(c => c.CreatorId == creatorId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<int> GetTotalContributors(int campaignId)
        {
            return await _CFS.Contributions
                                 .Where(c => c.CampaignId == campaignId && c.Status == "Confirmed") // optional: status filter
                                 .Select(c => c.ContributorId)
                                 .Distinct()
                                 .CountAsync();
        }

        //Get All contributors according to particular campaign
        //public async Task<List<Contribution>> GetContributorsByCampaignId(int campaignId)
        //{   
        //    return await _CFS.Contributions
        //        .Where(c => c.CampaignId == campaignId)
        //        .OrderByDescending(c => c.Date)
        //        .ToListAsync();
        //}

        public async Task<List<Contribution>> GetContributorsByCampaignId(int campaignId)
        {
            var contributors = await (
                from c in _CFS.Contributions
                join u in _CFS.Users on c.ContributorId equals u.UserId
                join camp in _CFS.Campaigns on c.CampaignId equals camp.CampaignId
                where c.CampaignId == campaignId
                orderby c.Date descending
                select new Contribution
                {
                    ContributionId = c.ContributionId,
                    CampaignId = c.CampaignId,
                    ContributorId = c.ContributorId,
                    Amount = c.Amount,
                    Date = c.Date,
                    TransactionId = c.TransactionId,
                    PaymentStatus = c.PaymentStatus,
                    Status = c.Status,
                    OrderId = c.OrderId,
                    PaymentId = c.PaymentId,

                    // Injecting contributor (User) data
                    Contributor = new User
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        ProfilePicture = u.ProfilePicture
                    },

                    // Optional: Include campaign data if needed
                    Campaign = new Campaign
                    {
                        CampaignId = camp.CampaignId,
                        Title = camp.Title,
                        MediaUrl = camp.MediaUrl,
                        Requirement = camp.Requirement,
                        RaisedAmount = camp.RaisedAmount
                    }
                }).ToListAsync();

            return contributors;
        }
 
    }
}
