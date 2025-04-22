using Crowd_Funding_Platform.Repositiories.Interfaces;
using Crowd_Funding_Platform.Models;
using Microsoft.EntityFrameworkCore;


namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly DbMain_CFS _context;

        public ActivityRepository(DbMain_CFS dbMain_CFS)
        {
            _context = dbMain_CFS;        
        }

        public object AddNewActivity(int userId, string activityType, string description, string? tableName = null, int? recordId = null)
        {
            try
            {
                var log = new TblAuditLog
                {
                    UserId = userId,
                    ActivityType = activityType,
                    Description = description,
                    TableName = tableName,
                    RecordId = recordId,
                    Timestamp = DateTime.UtcNow
                };

                _context.TblAuditLogs.Add(log);
                _context.SaveChanges();

                return new { success = true, message = "Activity logged successfully." };
            }
            catch (Exception ex)
            {
                // Optional: log the error somewhere (e.g., file, DB, external service)
                return new { success = false, message = "Failed to log activity.", error = ex.Message };
            }
        }

    }
}
