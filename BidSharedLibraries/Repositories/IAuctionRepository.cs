using BidSharedLibraries.Models;
namespace BidSharedLibraries.Repository
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetAuctionAsync(int auctionId);
        Task UpdateAuctionAsync(Auction auction);
    }
}