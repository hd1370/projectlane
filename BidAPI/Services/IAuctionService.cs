using BidAPI.Models;

namespace BidAPI.Services
{
    public interface IAuctionService
    {
        void AddBid(Bid bid);
        Task<(int winningUserId, decimal finalAmount)> FinalizeAuctionAsync(int auctionId);
    }
}
