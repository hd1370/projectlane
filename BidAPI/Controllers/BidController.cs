using BidSharedLibraries.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BidAPI.Controllers
{
    [Route("api/bids")]
    [ApiController]
    public class BidController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> PostBidAsync([FromBody] Bid bid)
        {
            if (bid == null) return BadRequest("Invalid bid");
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "bidsQueue", durable: true, exclusive: false, autoDelete: false,arguments: null);
            string message = JsonSerializer.Serialize(bid);
            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "bidsQueue", body: body);
            return Ok(new { message = "Bid received" });
        }
    }
}
