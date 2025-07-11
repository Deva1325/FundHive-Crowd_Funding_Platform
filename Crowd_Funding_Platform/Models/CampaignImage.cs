﻿using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class CampaignImage
{
    public int ImageId { get; set; }

    public int CampaignId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsThumbnail { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? UploadedDate { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;
}
