namespace Crowd_Funding_Platform.Models
{
    public class CreatorDashboardVM
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int CompletedCampaigns { get; set; }
        public decimal TotalRaisedAmount { get; set; }
        public int TotalContributors { get; set; }
        public List<Campaign> ChartCampaigns { get; set; } = new List<Campaign>();
    }
}
