using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class UserClassRepos : IUser
    {
        private readonly DbMain_CFS _CFS;

        public UserClassRepos(DbMain_CFS CFS)
        {
            _CFS = CFS;            
        }

        public async Task<List<Contribution>> GetAllContributorsAsync()
        {
            var contributors = await (
                from c in _CFS.Contributions
                join u in _CFS.Users on c.ContributorId equals u.UserId
                join camp in _CFS.Campaigns on c.CampaignId equals camp.CampaignId
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
                        RaisedAmount = camp.RaisedAmount,
                        Category = new Category
                        {
                            CategoryId = camp.Category.CategoryId,
                            Name = camp.Category.Name
                        }
                    }
                    
                }).ToListAsync();

            return contributors;
        }

        public async Task<List<CreatorApplication>> GetAllCreatorsAsync()
        {
            var creators = await (
                         from ca in _CFS.CreatorApplications
                         join u in _CFS.Users on ca.UserId equals u.UserId
                         select new CreatorApplication
                         {
                             ApplicationId = ca.ApplicationId,
                             Status = ca.Status,
                             SubmissionDate = ca.SubmissionDate,
                             AdminRemarks = ca.AdminRemarks,
                             DocumentType = ca.DocumentType,
                             DocumentPath = ca.DocumentPath,
                             User = new User
                             {
                                 UserId = u.UserId,
                                 ProfilePicture = u.ProfilePicture,
                                 Username = u.Username,
                                 Email = u.Email,
                                 PhoneNumber = u.PhoneNumber
                             }
                         }).ToListAsync();

            return creators;
        }
        public async Task<CreatorApplication?> GetCreatorsById(int id)
        {
            return await _CFS.CreatorApplications.FirstOrDefaultAsync(c => c.ApplicationId == id);
        }

        public async Task<bool> DeleteCreator(int id)
        {
            var creatorId = await _CFS.CreatorApplications.FindAsync(id);

            if (creatorId == null)
            {
                return false;
            }

            _CFS.CreatorApplications.Remove(creatorId);
            await _CFS.SaveChangesAsync();
            return true;
        }

        //My Contributions 

        //public async Task<List<Contribution>> GetMyContributions()
        //{
            
        //}


    }
}
