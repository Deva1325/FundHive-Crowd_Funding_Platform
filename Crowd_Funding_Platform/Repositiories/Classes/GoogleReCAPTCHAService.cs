using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crowd_Funding_Platform.Repositiories.Classes
{
    public class GoogleReCAPTCHAService : IGoogleReCAPTCHAService
    {
        private readonly GoogleReCAPTCHA _reCAPTCHA;
        private readonly HttpClient _httpClient;

        public GoogleReCAPTCHAService(IOptions<GoogleReCAPTCHA> reCAPTCHA, HttpClient httpClient)
        {
            _reCAPTCHA = reCAPTCHA.Value;
            _httpClient = httpClient;
        }

        public double Score { get; private set; }  // Store reCAPTCHA score


        public async Task<bool> VerifyToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token is empty or null.");
                return false;
            }

            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={_reCAPTCHA.SecretKey}&response={token}",
                null);

            Console.WriteLine($"response : {response} ");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to verify reCAPTCHA. HTTP error.");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ReCaptchaResponse>(json);

            Console.WriteLine($"reCAPTCHA Success: {result.Success}");
            Console.WriteLine($"Score: {result.Score}");
           
            // ✅ Score validation
            Score = result?.Score ?? 0.0;
            return result?.Success == true && result.Score >= 0.5;  // Score threshold
        }

    }

    public class ReCaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("score")]
        public float Score { get; set; }
    }
}
