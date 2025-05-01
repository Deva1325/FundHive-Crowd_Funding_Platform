namespace Crowd_Funding_Platform.Models
{
    public class AuditLogViewModel
    {
        public int LogId { get; set; }
        public string Username { get; set; }
        public string? ActivityType { get; set; }
        public string? Description { get; set; }
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsDeleted { get; set; } // add this
    }
}
