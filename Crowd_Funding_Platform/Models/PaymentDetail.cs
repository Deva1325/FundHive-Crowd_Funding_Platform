using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crowd_Funding_Platform.Models;

public partial class PaymentDetail
{
    public int PaymentId { get; set; }

    public int ContributionId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string? OrderId { get; set; }

    [NotMapped]
    public string? Signature { get; set; } // ✅ Add this property
    public string? RazorpayPaymentId { get; set; }

    public virtual Contribution Contribution { get; set; } = null!;
}
