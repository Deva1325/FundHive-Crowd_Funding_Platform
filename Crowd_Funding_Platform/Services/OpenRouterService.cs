using Newtonsoft.Json;
using System.Text;

namespace Crowd_Funding_Platform.Services
{
    public class OpenRouterService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "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"; // Your API Key

        public OpenRouterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateCampaignAsync(string keyword)
        {
            var requestBody = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[]
                {
            new { role = "user", content = $"Generate a creative crowdfunding title for the keyword: '{keyword}'." }
        }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Clear existing headers and add the required ones
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost"); // Use your domain if hosted later

            var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                // ✅ Parse JSON to extract only the generated content
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

                string aiContent = jsonResponse?.choices?[0]?.message?.content;

                if (!string.IsNullOrEmpty(aiContent))
                {
                    return aiContent.Trim(); // Only the title/description, not the whole JSON
                }
                else
                {
                    return "AI response content was empty.";
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter API call failed: {response.StatusCode}\nDetails: {errorContent}");
            }
            //if (response.IsSuccessStatusCode)
            //{
            //    var responseString = await response.Content.ReadAsStringAsync();
            //    return responseString;
            //}
            //else
            //{
            //    var errorContent = await response.Content.ReadAsStringAsync();
            //    throw new Exception($"OpenRouter API call failed: {response.StatusCode}\nDetails: {errorContent}");
            //}
        }


        //public async Task<string> GenerateCampaignAsync(string title, string description)
        //{
        //    var requestBody = new
        //    {
        //        prompt = $"Generate a campaign based on the following details: Title: {title}, Description: {description}",
        //        max_tokens = 500
        //    };

        //    var json = JsonConvert.SerializeObject(requestBody);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

        //    var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseString = await response.Content.ReadAsStringAsync();
        //        // You can parse the response here
        //        return responseString;
        //    }
        //    else
        //    {
        //        throw new Exception($"OpenRouter API call failed: {response.StatusCode}");
        //    }
        //}
    }
}
