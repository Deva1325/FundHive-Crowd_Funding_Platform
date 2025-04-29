using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class TblNotification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? Date { get; set; }

    public bool IsRead { get; set; }

    public int? Type { get; set; }

    public int? RelatedId { get; set; }

    public string? ModuleType { get; set; }

    public virtual User User { get; set; } = null!;
}
