namespace BidAPI.Models
{
    public enum AuctionStatus
    {
        Active,
        Finished
    }

    public class Auction
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        //public decimal StartingPrice { get; set; }
        public AuctionStatus Status { get; set; } = AuctionStatus.Active;
        public DateTime? FinishedAt { get; set; }
        public int WinnerId { get; set; }
        public decimal WinningAmount { get; set; }

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }

}
