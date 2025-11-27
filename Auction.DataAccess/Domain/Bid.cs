using Auction.DataAccess.Enums;
using Auction.DataAccess.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Auction.DataAccess.Domain
{
    //Teklifler
    public class Bid
    {
        [Key]
        public int BidId { get; set; }
        public decimal BidAmount { get; set; }
        public DateTime BidDate { get; set; }
        public string Status { get; set; } = BidStatus.Pending.ToString();
        public string? UserId { get; set; }
        [JsonIgnore]
        public ApplicationUser User { get; set; }
        public int VehicleId { get; set; }
        [JsonIgnore]

        public Vehicle Vehicle { get; set; }
    }
}
