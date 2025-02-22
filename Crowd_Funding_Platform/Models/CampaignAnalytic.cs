using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class CampaignAnalytic
{
    public int AnalyticsId { get; set; }

    public int CampaignId { get; set; }

    public int DailyViews { get; set; }

    public int DailyContributions { get; set; }

    public DateOnly? Timestamp { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;
}
