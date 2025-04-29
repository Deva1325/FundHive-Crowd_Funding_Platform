using Crowd_Funding_Platform.Models;

namespace Crowd_Funding_Platform.Services
{
    public class CampaignAiService
    {
        private readonly GeminiService _geminiService;
        private readonly OpenRouterService _openRouterService;

        public CampaignAiService(GeminiService geminiService, OpenRouterService openRouterService)
        {
            _geminiService = geminiService;
            _openRouterService = openRouterService;
        }

        public async Task<AIGeneratedResult> GenerateCampaignAsync(string category)
        {
            // Try Gemini first
            var geminiResult = await _geminiService.GenerateCampaignAsync(category);
            if (geminiResult.Success)
                return geminiResult;

            // Fallback to OpenRouter
            try
            {
                var (title, description, _) = await _openRouterService.GenerateCampaignFullAsync(category);

                return new AIGeneratedResult
                {
                    Success = true,
                    Source = "OpenRouter",
                    Title = title,
                    Description = description
                };
            }
            catch (Exception ex)
            {
                return new AIGeneratedResult
                {
                    Success = false,
                    ErrorMessage = $"OpenRouter error: {ex.Message}"
                };
            }
        }
     }
}