using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Repositiories.Interfaces.IManageCampaign;
using Microsoft.EntityFrameworkCore;
using static iTextSharp.text.pdf.events.IndexEvents;

namespace Crowd_Funding_Platform.Repositiories.Classes.ManageCampaign
{
    public class CreatorApplicationRepos : ICreatorApplicationRepos
    {
        private readonly INotificationService _notificationService;
        private readonly DbMain_CFS _dbMain_CFS;

        public CreatorApplicationRepos(DbMain_CFS dbMain_CFS,INotificationService notificationService)
        {
            _dbMain_CFS = dbMain_CFS;
            _notificationService=notificationService;
        }

        public async Task<object> ApplyForCreator(CreatorApplication creatorApp,IFormFile? ImageFile)
        {

            // ✅ Check latest application for the user
            var latestApplication = await _dbMain_CFS.CreatorApplications
                .Where(x => x.UserId == creatorApp.UserId)
                .OrderByDescending(x => x.SubmissionDate)
                .FirstOrDefaultAsync();

            if (latestApplication != null)
            {
                if (latestApplication.Status == "Pending")
                {
                    return new { success = false, message = "You have already submitted a request. Please wait for admin approval." };
                }

                if (latestApplication.Status == "Approved")
                {
                    return new { success = false, message = "You are already approved as a creator." };
                }

                // If rejected – allow resubmission (no block)
            }

            if (creatorApp.ImageFile == null || creatorApp.ImageFile.Length == 0)
                return new { success = false, message = "Document file is required." };

            // ✅ Check for file size limit (5 MB max)
            long maxFileSize = 5 * 1024 * 1024;
            if (creatorApp.ImageFile.Length > maxFileSize)
            {
                return new { success = false, message = "File size must be less than 5 MB." };
            }

            // ✅ Validate file extension
            string[] allowedExtensions = { ".pdf", ".jpg", ".png" };
            string fileExtension = Path.GetExtension(creatorApp.ImageFile.FileName).ToLower();

            if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                return new { success = false, message = "Invalid file format. Only PDF, JPG, and PNG are allowed." };

            // ✅ Ensure the upload folder exists
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Documents_Upload");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // ✅ Save the uploaded file with a unique name
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await creatorApp.ImageFile.CopyToAsync(stream);
            }

            creatorApp.DocumentPath = "/Documents_Upload/" + fileName;

            // ✅ Save to database
            await _dbMain_CFS.CreatorApplications.AddAsync(creatorApp);
            await _dbMain_CFS.SaveChangesAsync();

      //      await _notificationService.SendReminderNotificationAsync(
      //      creatorApp.UserId.Value,
      //                  $"Reminder: Your Application is sent to the Admin for is due in 3 days."
      //);

              await _notificationService.SendReminderNotificationAsync(
              2041,$"📥 New Creator Application Submitted by {creatorApp.User.Username} on {DateTime.Now:MMMM dd, yyyy hh:mm tt}."

        );
            return new { success = true, message = "Application submitted successfully!" };
        }

        //public async Task<object> ApplyForCreator(CreatorApplication creatorApp)
        //{
        //    if (creatorApp.ImageFile == null || creatorApp.ImageFile.Length == 0)
        //        return new { success = false, message = "Document file is required." };

        //    string[] allowedExtensions = { ".pdf", ".jpg", ".png" };
        //    string fileExtension = Path.GetExtension(creatorApp.ImageFile.FileName).ToLower();

        //    if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
        //        return new { success = false, message = "Invalid file format. Only PDF, JPG, and PNG are allowed." };

        //    string fileName = Guid.NewGuid().ToString() + fileExtension;
        //    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Documents_Upload", fileName);

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await creatorApp.ImageFile.CopyToAsync(stream);
        //    }
        //    creatorApp.DocumentPath = "/Documents_Upload/" + fileName;

        //    await _dbMain_CFS.CreatorApplications.AddAsync(creatorApp);
        //    await _dbMain_CFS.SaveChangesAsync();

        //    return new { success = true, message = "Application submitted successfully!" };
        //}


    }
}
