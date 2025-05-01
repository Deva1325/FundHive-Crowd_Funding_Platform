using Crowd_Funding_Platform.Repositiories.Interfaces;

namespace Crowd_Funding_Platform.Models
{
    public class HomePageViewModel
    {
        public List<Campaign>? Campaigns { get; set; }
    }
    public class TotalCounts
    {
        public int? TotalUsers { get; set; }
        public int?  TotalCampaigns { get; set; }
        public int? TotalContributions { get; set; }
        public decimal? TotalRaisedAmount { get; set; }
    }
}
