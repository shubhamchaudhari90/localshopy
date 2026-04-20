namespace localshopyNew.ViewModel
{
    public class ReviewViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal Rating { get; set; }
        public string? Comment { get; set; }

        public string? ShopName { get; set; }
        public string? ProductName { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;

        public double AverageRatings { get; set; }

        public int ReviewCount { get; set; }
    }
}
