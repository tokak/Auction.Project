using Auction.Business.Abstraction;
using Auction.Business.Dtos;
using Auction.Core.Models;
using Auction.DataAccess.Context;
using Auction.DataAccess.Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Auction.Business.Concrete
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private ApiResponse _response;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public PaymentHistoryService(ApiResponse response, ApplicationDbContext context, IMapper mapper)
        {
            _response = response;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse> CheckIsStatusForAuction(string userId, int vehicleId)
        {
            var response = await _context.PaymentHistories.FirstOrDefaultAsync(x => x.UserId == userId && x.VehicleId == vehicleId && x.IsActive == true);
            if (response != null)
            {
                _response.isSuccess = true;
                _response.Result = response;
                return _response;
            }
            _response.isSuccess = false;
            return _response;
        }

        public async Task<ApiResponse> CreatePaymentHistory(CreatePaymentHistoryDTO model)
        {
            if (model == null)
            {
                _response.isSuccess = false;
                _response.ErrorMessages.Add("Model bazı alanları içermiyor");
                return _response;
            }
            else
            {
                var objDTO = _mapper.Map<PaymentHistory>(model);
                objDTO.PayDate = DateTime.Now;
                objDTO.IsActive = true;
                _context.PaymentHistories.Add(objDTO);
                if (await _context.SaveChangesAsync() > 0)
                {
                    _response.isSuccess = true;
                    _response.Result = model;
                    return _response;

                }
                _response.isSuccess = false;
                _response.ErrorMessages.Add("Bir hata oluştu!");
                return _response;

            }


        }
    }
}
