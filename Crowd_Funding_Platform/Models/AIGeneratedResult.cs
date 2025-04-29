namespace Crowd_Funding_Platform.Models
{
    public class AIGeneratedResult
    {
        public bool Success { get; set; }
        public string Source { get; set; } // "Gemini" or "OpenRouter"
        public string Title { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
        public string Category { get; set; }
    }
}
