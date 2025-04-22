using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class TblAuditLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string? Description { get; set; }

    public string? ActivityType { get; set; }

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual User User { get; set; } = null!;
}
