using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class TblContact
{
    public int ContactId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Message { get; set; }

    public DateTime SubmittedAt { get; set; }
}
