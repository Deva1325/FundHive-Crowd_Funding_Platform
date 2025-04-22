using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Net;

namespace Crowd_Funding_Platform.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public OpenAIService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"];
            _model = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
        }

        public async Task<string> GenerateCampaignContent(string category, string requirement)
        {
            // --- FAKE AI RESPONSE FOR TESTING ---
            await Task.Delay(1000); // Simulate network delay

            return $"Title: Help Us Change Lives in the {category} Sector\n" +
                   $"Description: We aim to raise {requirement} to fund a meaningful project that addresses critical needs in the {category} space. Your support can make a real difference!";
        }
    }

    //public class OpenAIService
    //{
    //    private readonly HttpClient _httpClient;
    //    private readonly string _apiKey;
    //    private readonly string _model;

    //    public OpenAIService(IConfiguration configuration)
    //    {
    //        _httpClient = new HttpClient();
    //        _apiKey = configuration["OpenAI:ApiKey"];
    //        _model = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
    //    }

    //    public async Task<string> GenerateCampaignContent(string category, string requirement)
    //    {
    //        var prompt = $"Suggest a compelling crowdfunding campaign title and description for the following:\n" +
    //                     $"Category: {category}\n" +
    //                     $"Funding Goal: {requirement}\n" +
    //                     $"Output format: \nTitle: <title>\nDescription: <description>";

    //        var requestBody = new
    //        {
    //            model = _model,
    //            messages = new[]
    //            {
    //            new { role = "system", content = "You are a helpful assistant that writes engaging crowdfunding campaigns." },
    //            new { role = "user", content = prompt }
    //        },
    //            max_tokens = 300,
    //            temperature = 0.8
    //        };

    //        // ✅ Create new request every time to avoid reuse error
    //        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
    //        {
    //            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
    //        };
    //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

    //        var response = await _httpClient.SendAsync(request);
    //        response.EnsureSuccessStatusCode();

    //        var responseString = await response.Content.ReadAsStringAsync();
    //        using var jsonDoc = JsonDocument.Parse(responseString);
    //        var result = jsonDoc.RootElement
    //            .GetProperty("choices")[0]
    //            .GetProperty("message")
    //            .GetProperty("content")
    //            .GetString();

    //        return result;
    //    }
    //}

}
