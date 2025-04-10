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

        //public async Task<List<Contribution>> GetAllContributorsAsync()
        //{
        //    var contributor = await (
        //                 from co in _CFS.Contributions
        //                 join u in _CFS.Users on co.ContributorId equals u.UserId
        //                 select new Contribution
        //                 {
        //                     ContributionId=co.ContributionId,



        //                     ApplicationId = ca.ApplicationId,
        //                     Status = ca.Status,
        //                     SubmissionDate = ca.SubmissionDate,
        //                     AdminRemarks = ca.AdminRemarks,
        //                     DocumentType = ca.DocumentType,
        //                     DocumentPath = ca.DocumentPath,
        //                     User = new User
        //                     {
        //                         UserId = u.UserId,
        //                         ProfilePicture = u.ProfilePicture,
        //                         Username = u.Username,
        //                         Email = u.Email,
        //                         PhoneNumber = u.PhoneNumber
        //                     }
        //                 }).ToListAsync();

        //    return creators;
        //}

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


        

    }
}
