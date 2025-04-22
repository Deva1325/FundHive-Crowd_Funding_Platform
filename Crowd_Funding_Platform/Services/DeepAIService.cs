using System.Text.Json;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Crowd_Funding_Platform.Services
{
    public class DeepAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public DeepAIService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["DeepAI:ApiKey"];
        }

        public async Task<string> GenerateTextAsync(string input)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.deepai.org/api/text-generator");
            if (!_httpClient.DefaultRequestHeaders.Contains("api-key"))
            {
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            }

            var content = new Dictionary<string, string>
        {
            { "text", input }
        };
            request.Content = new FormUrlEncodedContent(content);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Using API key: " + _apiKey);

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseString);
            return jsonDoc.RootElement.GetProperty("output").GetString();
        }
    }

}
