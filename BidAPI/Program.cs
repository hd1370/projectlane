
using BidAPI.Repository;
using BidAPI.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

namespace BidAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
            logger.Debug("Starting app.");

            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Logging.ClearProviders();
            builder.Host.UseNLog(); 
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IAuctionService, AuctionService>();
            builder.Services.AddSignalR();
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173/")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true; 
            });

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowFrontend");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<AuctionHub>("/auctionHub");
            app.Run();
        }
    }
}
