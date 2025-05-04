using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crowd_Funding_Platform.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    [NotMapped]
    public decimal TotalContributions { get; set; } // <-- Add this

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
