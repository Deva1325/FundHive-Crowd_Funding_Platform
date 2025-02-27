using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crowd_Funding_Platform.Models;

public partial class CreatorApplication
{
    public int ApplicationId { get; set; }

    public int? UserId { get; set; }

    public string? DocumentType { get; set; }

    public string? DocumentPath { get; set; }

    public string? Status { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? StatusUpdatedDate { get; set; }

    public string? AdminRemarks { get; set; }

    [NotMapped]
    public FormFile? ImageFile { get; set; }

    [NotMapped]
    public virtual User? User { get; set; }
}
