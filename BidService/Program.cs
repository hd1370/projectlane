using BidSharedLibraries.Repository;
using Microsoft.EntityFrameworkCore;

namespace BidService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
            builder.Services.AddHostedService<AuctionWorker>();
            var host = builder.Build();
            host.Run();
        }
    }
}