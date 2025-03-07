using BidAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace BidAPI.Repository
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AppDbContext _context;

        public AuctionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Auction?> GetAuctionAsync(int auctionId)
        {
            return await _context.Auctions.FindAsync(auctionId);
        }

        public async Task UpdateAuctionAsync(Auction auction)
        {
            _context.Auctions.Update(auction);
            await _context.SaveChangesAsync();
        }
    }
}