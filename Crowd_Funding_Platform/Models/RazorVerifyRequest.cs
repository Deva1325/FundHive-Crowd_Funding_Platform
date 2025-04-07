namespace Crowd_Funding_Platform.Models
{
    public class RazorVerifyRequest
    {
        public string? razorpay_payment_id { get; set; }
        public string? razorpay_order_id { get; set; }
        public string? razorpay_signature { get; set; }
        public int campaignId { get; set; }
        public int amount { get; set; }
    }
}
