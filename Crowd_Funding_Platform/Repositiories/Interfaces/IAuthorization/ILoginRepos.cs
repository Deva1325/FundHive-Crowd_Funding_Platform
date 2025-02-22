namespace Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization
{
    public interface ILoginRepos
    {
        Task<object> AuthenticateUser(string EmailOrUsername, string password);
        Task<object> TokenSenderViaEmail(string email);
        Task<object> ResetPassword(string creds, string newPassword);
        Task<object> LogoutUser(string email);
    }
}
