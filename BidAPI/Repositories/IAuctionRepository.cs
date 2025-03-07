using BidAPI.Models;
namespace BidAPI.Repository
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetAuctionAsync(int auctionId);
        Task UpdateAuctionAsync(Auction auction);
    }
}