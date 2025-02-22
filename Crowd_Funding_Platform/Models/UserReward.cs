using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class UserReward
{
    public int UserRewardId { get; set; }

    public int UserId { get; set; }

    public int RewardId { get; set; }

    public decimal TotalContribution { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Reward Reward { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
