using System;
using System.Collections.Generic;

namespace Crowd_Funding_Platform.Models;

public partial class Contribution
{
    public int ContributionId { get; set; }

    public int CampaignId { get; set; }

    public int ContributorId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? Date { get; set; }

    public string? TransactionId { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? PaymentId { get; set; }

    public string? OrderId { get; set; }

    public string? Status { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual User Contributor { get; set; } = null!;

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
}
