namespace Crowd_Funding_Platform.Models
{
    public class GoogleSignupModel
    {
        public string Email { get; set; }
        public string Username { get; set; }

        //public string temppassword { get; set; }

        public string PasswordHash { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
