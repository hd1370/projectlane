using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using BidSharedLibraries.Models;
using BidSharedLibraries.Repository;

namespace BidService
{
    public class AuctionWorker : BackgroundService
    {
        private readonly ILogger<AuctionWorker> _logger;
        private IChannel _channel;
        private IConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;


        private readonly ConcurrentDictionary<int, List<Bid>> _auctionBids = new();
        private readonly ConcurrentDictionary<int, Timer> _auctionTimers = new();

        public AuctionWorker(ILogger<AuctionWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(queue: "bidsQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _logger.LogInformation("Listening ot 'bidsQueue'");
            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var bid = JsonSerializer.Deserialize<Bid>(message);
                    if (bid != null)
                    {
                        ProcessBid(bid);
                        _logger.LogInformation("Received bid: Auction={AuctionId}, User={UserId}, Amount={Amount}",
                            bid.AuctionId, bid.UserId, bid.Amount);
                    }
                    await _channel.BasicAckAsync(ea.DeliveryTag, false).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing bid");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true).ConfigureAwait(false);
                }
            };
            _channel.BasicConsumeAsync(queue: "bidsQueue", autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        private void ProcessBid(Bid bid)
        {
            _auctionBids.AddOrUpdate(bid.AuctionId,
                new List<Bid> { bid },
                (key, existingList) =>
                {
                    existingList.Add(bid);
                    return existingList;
                });

            // Start a timer to simulate end of bid
            if (!_auctionTimers.ContainsKey(bid.AuctionId))
            {
                var timer = new Timer(state =>
                {
                    int auctionId = (int)state;
                    _ = FinalizeAuctionAsync(auctionId);
                    _auctionTimers.TryRemove(auctionId, out _);
                },
                bid.AuctionId,
                TimeSpan.FromMinutes(1), // Auction duration
                Timeout.InfiniteTimeSpan);
                _auctionTimers.TryAdd(bid.AuctionId, timer);
            }
        }

        private async Task FinalizeAuctionAsync(int auctionId)
        {
            if (_auctionBids.TryRemove(auctionId, out var bids))
            {
                var winningBid = bids.OrderByDescending(b => b.Amount).FirstOrDefault();
                if (winningBid != null)
                {
                    _logger.LogInformation("Auction {AuctionId} finalized. Winner: UYser={UserId}, Final Amount={Amount}",
                        auctionId, winningBid.UserId, winningBid.Amount);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
                        var auction = await auctionRepository.GetAuctionAsync(auctionId);
                        if (auction == null)
                        {
                            // Todo error handling
                            return;
                        }
                        auction.WinnerId = winningBid.UserId;
                        auction.FinishedAt = DateTime.UtcNow;
                        auction.Status = AuctionStatus.Finished;
                        auction.WinningAmount = winningBid.Amount;
                        await auctionRepository.UpdateAuctionAsync(auction);
                    }
                }
                else
                {
                    // Todo better error handling
                    _logger.LogInformation("No valid bid for {auctionId}", auctionId);
                }
            }
            else
            {
                // Todo better error handling
                _logger.LogInformation("No bids in memory for {auctionId}", auctionId);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.CloseAsync();
            _connection.CloseAsync();
            return base.StopAsync(cancellationToken);
        }
    }
}
