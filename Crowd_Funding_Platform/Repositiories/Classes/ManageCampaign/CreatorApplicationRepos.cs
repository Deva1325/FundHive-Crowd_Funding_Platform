using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;

namespace Crowd_Funding_Platform.Repositiories.Classes.ManageCampaign
{
    public class CreatorApplicationRepos : ICreatorApplicationRepos
    {

        private readonly DbMain_CFS _dbMain_CFS;

        public CreatorApplicationRepos(DbMain_CFS dbMain_CFS)
        {
            _dbMain_CFS = dbMain_CFS;
        }

        public async Task<object> ApplyForCreator(CreatorApplication creatorApp)
        {
            if (creatorApp.ImageFile == null || creatorApp.ImageFile.Length == 0)
                return new { success = false, message = "Document file is required." };

            string[] allowedExtensions = { ".pdf", ".jpg", ".png" };
            string fileExtension = Path.GetExtension(creatorApp.ImageFile.FileName).ToLower();

            if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                return new { success = false, message = "Invalid file format. Only PDF, JPG, and PNG are allowed." };

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Documents_Upload", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await creatorApp.ImageFile.CopyToAsync(stream);
            }
            creatorApp.DocumentPath = "/Documents_Upload/" + fileName;

            await _dbMain_CFS.CreatorApplications.AddAsync(creatorApp);
            await _dbMain_CFS.SaveChangesAsync();

            return new { success = true, message = "Application submitted successfully!" };
        }


    }
}
