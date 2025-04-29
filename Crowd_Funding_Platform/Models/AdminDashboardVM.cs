namespace Crowd_Funding_Platform.Models
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalCampaigns { get; set; }
        public int TotalContributions { get; set; }
        public decimal TotalRaisedAmount { get; set; }
        public int TotalCreatorApplications { get; set; }
        public int PendingCreatorApplications { get; set; }
        public int TotalPayments { get; set; }
        public int TotalRewards { get; set; }


        public List<ChartDataPoint> RaisedPerWeek { get; set; }
        public List<ChartDataPoint> TopCampaigns { get; set; }
        public List<ChartDataPoint> PaymentStatusDistribution { get; set; }
        public List<ChartDataPoint> ContributionsByCategory { get; set; }

        public List<Campaign> LatestCampaigns { get; set; }
        public List<CreatorApplication> PendingApplications { get; set; }
        public List<Contribution> RecentContributions { get; set; }
        public List<ChartDataPoint> TopContributors { get; set; }

    }

}
