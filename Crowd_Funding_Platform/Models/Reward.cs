using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class Reward
{
    public int RewardId { get; set; }

    public string RewardBatch { get; set; } = null!;

    public decimal RequiredAmount { get; set; }

    public string? RewardDescription { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual ICollection<UserReward> UserRewards { get; set; } = new List<UserReward>();
}
