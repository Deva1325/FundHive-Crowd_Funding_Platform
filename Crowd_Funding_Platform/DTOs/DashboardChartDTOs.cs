namespace Crowd_Funding_Platform.DTOs
{
    public class DashboardChartDTOs
    {

    }

    public class ContributionTrendDTO
    {
        public string Month { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CampaignInsightDTO
    {
        public string CampaignTitle { get; set; }
        public decimal AvgDonation { get; set; }
        public int TotalContributors { get; set; }
        public decimal RaisedPercent { get; set; }
    }
}
