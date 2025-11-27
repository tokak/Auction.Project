namespace Auction.Business.Dtos
{
    public class CreateBidDTO
    {
        public decimal BidAmount { get; set; }
        public string? UserId { get; set; }
        public int VehicleId { get; set; }
    }
}
