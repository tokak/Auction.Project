using Auction.Business.Dtos;
using Auction.Core.Models;

namespace Auction.Business.Abstraction
{
    public interface IPaymentHistoryService
    {
        Task<ApiResponse> CreatePaymentHistory(CreatePaymentHistoryDTO model);
        Task<ApiResponse> CheckIsStatusForAuction(string userId, int vehicleId);
    }
}
