using BidSharedLibraries.Models;
using Microsoft.EntityFrameworkCore;

namespace BidSharedLibraries.Repository
{
    public class AppDbContext : DbContext
    {
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}