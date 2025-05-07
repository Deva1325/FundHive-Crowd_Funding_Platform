using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class CampaignAchievement
{
    public int AchievementId { get; set; }

    public int? CampaignId { get; set; }

    public int? CreatorId { get; set; }

    public bool? IsGoalAchieved { get; set; }

    public string? CertificatePath { get; set; }

    public DateTime? AwardDate { get; set; }
}
