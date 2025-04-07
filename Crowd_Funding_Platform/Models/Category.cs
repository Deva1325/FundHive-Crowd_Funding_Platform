using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }


    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
