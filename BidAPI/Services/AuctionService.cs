using BidAPI.Models;
using BidAPI.Services;
using System.Collections.Concurrent;

namespace BidAPI.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly ConcurrentDictionary<int, List<Bid>> _auctionBids = new();

        public void AddBid(Bid bid)
        {
            _auctionBids.AddOrUpdate(
                bid.AuctionId,
                new List<Bid> { bid },
                (key, existingList) =>
                {
                    existingList.Add(bid);
                    return existingList;
                });
        }

        public async Task<(int winningUserId, decimal finalAmount)> FinalizeAuctionAsync(int auctionId)
        {
            if (_auctionBids.TryRemove(auctionId, out var bids))
            {
                var csvLines = new List<string> { "AuctionId;UserId;Amount;Timestamp" };
                foreach (var bid in bids)
                {
                    csvLines.Add($"{bid.AuctionId};{bid.UserId};{bid.Amount};{bid.Timestamp:O}");
                }
                string csvData = string.Join(Environment.NewLine, csvLines);
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string fileName = $"{auctionId}_{timestamp}.csv";
                string logsFolder = "Logs";
                if (!Directory.Exists(logsFolder))
                {
                    Directory.CreateDirectory(logsFolder);
                }
                string filePath = Path.Combine(logsFolder, fileName);
                await File.WriteAllTextAsync(filePath, csvData);

                var winningBid = bids.OrderByDescending(b => b.Amount).FirstOrDefault();
                int winningUserId = winningBid.UserId;
                decimal finalAmount = winningBid.Amount;

                return (winningUserId, finalAmount);
            }
            else
            {
                return (0, 0);
            }
        }

    }
}