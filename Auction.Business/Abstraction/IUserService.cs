using Auction.Business.Dtos;
using Auction.Core.Models;

namespace Auction.Business.Abstraction
{
    public  interface IUserService
    {
        Task<ApiResponse> Register(RegisterRequestDto model);
        Task<ApiResponse> Login(LoginRequestDto model);
    }
}
