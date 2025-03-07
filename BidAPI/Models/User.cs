namespace BidAPI.Models
{
    public class User
    {
        public int Id { get; set; } //todo should work with guids, lil more overhead to setup
        //public string Username { get; set; } = string.Empty;
        //public string Email { get; set; } = string.Empty;
        //public string PasswordHash { get; set; } = string.Empty; //todo Should contain a nicely hashed password (out of scope)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Bid> Bids { get; set; } = new();
    }
}
