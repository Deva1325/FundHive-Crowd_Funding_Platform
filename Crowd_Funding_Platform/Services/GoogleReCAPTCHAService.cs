using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Crowd_Funding_Platform.Services
{
    public class GoogleReCAPTCHAService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GoogleReCAPTCHAService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyReCAPTCHA(string token)
        {
            var secretKey = _configuration["GoogleReCAPTCHA:SecretKey"];
            var response = await _httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}");

            var captchaResult = JsonConvert.DeserializeObject<ReCAPTCHAResponse>(response);
            return captchaResult.Success && captchaResult.Score >= 0.5;  // Score threshold for V3
        }
    }

    public class ReCAPTCHAResponse
    {
        public bool Success { get; set; }
        public double Score { get; set; }
        public string[] ErrorCodes { get; set; }
    }
}
