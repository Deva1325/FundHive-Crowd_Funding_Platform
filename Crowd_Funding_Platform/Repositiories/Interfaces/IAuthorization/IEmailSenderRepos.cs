namespace Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization
{
    public interface IEmailSenderRepos
    {
        Task SendEmailAsync(string toEmail, string subject, string body, string emailType);

        string GenerateOtp();
    }
}
