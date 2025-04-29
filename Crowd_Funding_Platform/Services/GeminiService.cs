using Crowd_Funding_Platform.Models;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Crowd_Funding_Platform.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly DbMain_CFS _CFS;


        public GeminiService(HttpClient httpClient,IConfiguration configuration, DbMain_CFS cFS)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["Gemini:ApiKey"];
            _model = configuration["Gemini:Model"];
            _CFS = cFS;
        }

        public async Task<AIGeneratedResult> GenerateCampaignAsync(string keyword1)
        {

            var allowedCategories = await _CFS.Categories
    .Select(c => c.Name).ToListAsync();

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Generate a creative and engaging crowdfunding campaign title,category and short description (max 200 words) for the keyword : {keyword1}. Format: Title: ... Description: ... Category:<choose exactly one category from this list: {string.Join(", ", allowedCategories)}>" }
                        }
                    }
                }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}")
            {
                Content = requestContent
            };

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new AIGeneratedResult
                    {
                        Success = false,
                        ErrorMessage = $"Gemini error: {responseBody}"
                    };
                }

                using var jsonDoc = JsonDocument.Parse(responseBody);
                var text = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                string title = "", description = "";

                //var lines = text.Split('\n');
                //foreach (var line in lines)
                //{
                //    if (line.ToLower().StartsWith("Title:"))
                //        title = line.Substring("Title:".Length).Trim();
                //    else if (line.ToLower().StartsWith("Description:"))
                //        description = line.Substring("Description:".Length).Trim();
                //}


                var parsedData = ParseResponse(text);

                //if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
                //    description = text; // fallback if not parsed cleanly

                return new AIGeneratedResult
                {
                    Success = true,
                    Source = "Gemini",
                    Title = parsedData.ContainsKey("Title") ? parsedData["Title"] : null,
                    Category = parsedData.ContainsKey("Category") ? parsedData["Category"] : null,
                    Description = parsedData.ContainsKey("Description") ? parsedData["Description"] : text // fallback if not found
                };
            }
            catch (Exception ex)
            {
                return new AIGeneratedResult
                {
                    Success = false,
                    ErrorMessage = $"Gemini exception: {ex.Message}"
                };
            }
        }
        static Dictionary<string, string> ParseResponse(string response)
        {
            var result = new Dictionary<string, string>();
            var lines = response.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var pattern = @"^\*\*(.+?):\*\*\s*(.+)$";
            foreach (var line in lines)
            {
                var match = Regex.Match(line.Trim(), pattern);
                if (match.Success)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();
                    result[key] = value;
                }
            }

            return result;
        }
    }
}