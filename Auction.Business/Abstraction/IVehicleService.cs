using Auction.Business.Dtos;
using Auction.Core.Models;

namespace Auction.Business.Abstraction
{
    public interface IVehicleService
    {
        Task<ApiResponse> CreateVehicle(CreateVehicleDto model);
        Task<ApiResponse> GetVehicle();
        Task<ApiResponse> UpdateVehicleResponse(int vehicleId,UpdateVehicleDto model);
        Task<ApiResponse> DeleteVehicle(int vehicleId);
        Task<ApiResponse> GetVehicleById(int vehicleId);
        Task<ApiResponse> ChangeVehivle(int vehicleId);
    }
}
