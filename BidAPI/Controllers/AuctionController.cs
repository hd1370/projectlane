using BidAPI.Models;
using BidAPI.Repository;
using BidAPI.Services;
using Microsoft.AspNetCore.Mvc;
namespace BidAPI.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IAuctionRepository _auctionRepository;

        public AuctionController(IAuctionService auctionService, IAuctionRepository auctionRepository)
        {
            _auctionService = auctionService;
            _auctionRepository = auctionRepository;
        }

        [HttpPost("{auctionId}/finish")]
        public async Task<IActionResult> FinishAuction(int auctionId)
        {
            var (winningUserId, finalAmount) = await _auctionService.FinalizeAuctionAsync(auctionId);
            if (winningUserId == null || finalAmount == null)
            {
                return NotFound(new { message = "No bids found for the auction." });
            }

            var auction = await _auctionRepository.GetAuctionAsync(auctionId);
            if (auction == null)
            {
                return NotFound(new { message = "Auction not found in DB." });
            }

            auction.WinnerId = winningUserId;
            auction.FinishedAt = DateTime.UtcNow;
            auction.Status = AuctionStatus.Finished;
            auction.WinningAmount = finalAmount;  

            await _auctionRepository.UpdateAuctionAsync(auction);
            return Ok(new { message = "Auction finalized.", winner = winningUserId, finalAmount = finalAmount });
        }
    }
}