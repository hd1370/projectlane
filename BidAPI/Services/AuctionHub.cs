using BidAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace BidAPI.Services
{
    public class AuctionHub : Hub
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionHub> _logger;

        public AuctionHub(IAuctionService auctionService, ILogger<AuctionHub> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
        }

        public async Task PlaceBid(int auctionId, int userId, decimal amount)
        {
            try
            {
                var bid = new Bid
                {
                    AuctionId = auctionId,
                    UserId = userId,
                    Amount = amount,
                    Timestamp = DateTime.UtcNow
                };

                _auctionService.AddBid(bid);
                _logger.LogInformation("Bid added: Auction {AuctionId}, User {UserId}, Amount {Amount}", auctionId, userId, amount);
                await Clients.All.SendAsync("ReceiveBid", auctionId, userId, amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placig bid for auction {AuctionId}", auctionId);
                throw;
            }
        }
    }
}
