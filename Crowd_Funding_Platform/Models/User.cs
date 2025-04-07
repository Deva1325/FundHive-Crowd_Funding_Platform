using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crowd_Funding_Platform.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool? EmailVerified { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Otp { get; set; }

    public DateTime? Otpexpiry { get; set; }

    public string? ProfilePicture { get; set; }

    public bool? IsAdmin { get; set; }

    public bool? IsCreatorApproved { get; set; }

    [NotMapped]
    public string? ConfirmPassword { get; set; } = null!;

    [NotMapped]
    public FormFile? ImageFile { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Address { get; set; }

    public string? ProfileBio { get; set; }

    public string? Website { get; set; }

    public string? InstagramLink { get; set; }

    public string? FaceBookLink { get; set; }
    public virtual ICollection<AdminLog> AdminLogs { get; set; } = new List<AdminLog>();

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    public virtual ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();

    public virtual ICollection<CreatorApplication> CreatorApplications { get; set; } = new List<CreatorApplication>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<UserReward> UserRewards { get; set; } = new List<UserReward>();
}
