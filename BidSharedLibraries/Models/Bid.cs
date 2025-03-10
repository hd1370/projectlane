namespace BidSharedLibraries.Models
{

    public class Bid
    {
        public int Id { get; set; }

        public int AuctionId { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


    }
}
