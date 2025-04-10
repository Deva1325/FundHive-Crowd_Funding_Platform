using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crowd_Funding_Platform.Models;

public partial class Campaign
{
    public int CampaignId { get; set; }

    public int CreatorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Requirement { get; set; }

    public decimal? RaisedAmount { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int CategoryId { get; set; }

    public string? MediaUrl { get; set; }

    public string? Status { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }  // Updated type

    [NotMapped]
    public int TotalContributors { get; set; }

    public virtual ICollection<CampaignAnalytic> CampaignAnalytics { get; set; } = new List<CampaignAnalytic>();

    public virtual ICollection<CampaignImage> CampaignImages { get; set; } = new List<CampaignImage>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();

    public virtual User Creator { get; set; } = null!;
}
