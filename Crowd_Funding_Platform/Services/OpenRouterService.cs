using Crowd_Funding_Platform.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Crowd_Funding_Platform.Services
{
    public class OpenRouterService
    {
        private readonly HttpClient _httpClient;
        private readonly DbMain_CFS _CFS;

        private const string ApiKey = "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"; // Your API Key

        public OpenRouterService(HttpClient httpClient,DbMain_CFS dbMain_CFS)
        {
            _httpClient = httpClient;
            _CFS = dbMain_CFS;
        }

        //public async Task<(string Title, string Description, string Category)> GenerateCampaignFullAsync(string keyword)
        //{
        //    var requestBody = new
        //    {
        //        model = "mistralai/mistral-7b-instruct",
        //        messages = new[]
        //        {
        //    new { role = "user", content = $"Generate a creative crowdfunding campaign for the keyword: '{keyword}'. Provide the output in the following format:\n\nTitle: <title>\nDescription: <description>\nCategory: <category>" }
        //}
        //    };

        //    var json = JsonConvert.SerializeObject(requestBody);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    _httpClient.DefaultRequestHeaders.Clear();
        //    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
        //    _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");

        //    var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseString = await response.Content.ReadAsStringAsync();
        //        dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

        //        string aiContent = jsonResponse?.choices?[0]?.message?.content;

        //        if (!string.IsNullOrEmpty(aiContent))
        //        {
        //            // 🧠 Parse the AI content
        //            string title = "";
        //            string description = "";
        //            string category = "";

        //            var lines = aiContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        //            foreach (var line in lines)
        //            {
        //                if (line.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
        //                    title = line.Substring(6).Trim();
        //                else if (line.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
        //                    description = line.Substring(12).Trim();
        //                else if (line.StartsWith("Category:", StringComparison.OrdinalIgnoreCase))
        //                    category = line.Substring(9).Trim();
        //            }

        //            return (title, description, category);
        //        }
        //        else
        //        {
        //            throw new Exception("AI response was empty.");
        //        }
        //    }
        //    else
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        throw new Exception($"OpenRouter API call failed: {response.StatusCode}\nDetails: {errorContent}");
        //    }
        //}

        public async Task<(string Title, string Description, string Category)> GenerateCampaignFullAsync(string keyword)
        {
            //var allowedCategories = new[] { "Healthcare", "Education", "Environment", "Technology", "Community", "Art", "Animal Welfare", "Disaster Relief" };

            var allowedCategories = await _CFS.Categories
    .Select(c => c.Name)
    .ToListAsync();

            var prompt = $$"""
                        Generate a creative crowdfunding campaign idea based on the keyword: "{{keyword}}".

                        Respond ONLY in valid JSON without any extra text or explanation.

                        Example format:
                        ```json
                        {
                            "title": "<short and catchy title>",
                            "description": "<detailed and compelling description>",
                            "category": "<choose exactly one category from this list: {{string.Join(", ", allowedCategories)}}>"
                        }
                          Only return valid JSON without any extra text or explanation.
                        """;

            var requestBody = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[]
                {
            new { role = "user", content = prompt }
        }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");

            var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

                string aiContent = jsonResponse?.choices?[0]?.message?.content;

                if (!string.IsNullOrEmpty(aiContent))
                {
                    try
                    {
                        // Extract the JSON part using regex (safer)
                        var match = Regex.Match(aiContent, @"\{[\s\S]*?\}");
                        if (!match.Success)
                            throw new Exception("JSON object not found in AI response.");

                        var cleanJson = match.Value;

                        var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(cleanJson);

                        string title = parsed.ContainsKey("title") ? parsed["title"].Trim() : "";
                        string description = parsed.ContainsKey("description") ? parsed["description"].Trim() : "";
                        string category = parsed.ContainsKey("category") ? parsed["category"].Trim() : "";

                        return (title, description, category);
                    }
                    catch
                    {
                        throw new Exception("Failed to parse AI JSON response.");
                    }
                }
                else
                {
                    throw new Exception("AI response was empty.");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter API call failed: {response.StatusCode}\nDetails: {errorContent}");
            }
        }

        
    }
}

//public async Task<string> GenerateCampaignAsync(string keyword)
//{
//    var requestBody = new
//    {
//        model = "mistralai/mistral-7b-instruct",
//        messages = new[]
//        {
//    new { role = "user", content = $"Generate a creative crowdfunding title for the keyword: '{keyword}'." }
//}
//    };

//    var json = JsonConvert.SerializeObject(requestBody);
//    var content = new StringContent(json, Encoding.UTF8, "application/json");

//    // Clear existing headers and add the required ones
//    _httpClient.DefaultRequestHeaders.Clear();
//    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
//    _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost"); // Use your domain if hosted later

//    var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

//    if (response.IsSuccessStatusCode)
//    {
//        var responseString = await response.Content.ReadAsStringAsync();

//        // ✅ Parse JSON to extract only the generated content
//        dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

//        string aiContent = jsonResponse?.choices?[0]?.message?.content;

//        if (!string.IsNullOrEmpty(aiContent))
//        {
//            return aiContent.Trim(); // Only the title/description, not the whole JSON
//        }
//        else
//        {
//            return "AI response content was empty.";
//        }
//    }
//    else
//    {
//        var errorContent = await response.Content.ReadAsStringAsync();
//        throw new Exception($"OpenRouter API call failed: {response.StatusCode}\nDetails: {errorContent}");
//    }

//}