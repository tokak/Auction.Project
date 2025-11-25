using Auction.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace Auction.DataAccess.Domain
{
    public class PaymentHistory
    {
        [Key]
        public int PaymentId { get; set; }
        public bool IsActive { get; set; }
        public DateTime PayDate { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
