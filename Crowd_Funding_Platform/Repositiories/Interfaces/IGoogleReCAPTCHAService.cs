namespace Crowd_Funding_Platform.Repositiories.Interfaces
{
    public interface IGoogleReCAPTCHAService
    {
        Task<bool> VerifyToken(string token);
        double Score { get; }  // Add Score property
    }
}
