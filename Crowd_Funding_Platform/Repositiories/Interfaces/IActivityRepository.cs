using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IActivityRepository
    {
        object AddNewActivity(int userId, string activityType, string description, string tableName = null, int? recordId = null);

        List<AuditLogViewModel> GetAllAuditLogs();
    }
}
