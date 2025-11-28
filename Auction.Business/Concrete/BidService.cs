using Auction.Business.Abstraction;
using Auction.Business.Dtos;
using Auction.Core.MailHelper;
using Auction.Core.Models;
using Auction.DataAccess.Context;
using Auction.DataAccess.Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction.Business.Concrete
{
    public class BidService : IBidService
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ApiResponse response;
        private readonly IMailService _mailService;

        public BidService(ApplicationDbContext context, IMailService mailService, IMapper mapper, ApiResponse response)
        {
            this.context = context;
            this.mapper = mapper;
            this.response = response;
            _mailService = mailService;
        }

        public async Task<ApiResponse> AutomaticallyCreateBid(CreateBidDTO model)
        {
            var isPaid = await CheckIsPaidAuction(model.UserId, model.VehicleId);
            if (!isPaid)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add("Lütfen ödeme işlemini tamamladıktan sonra devam edin.");
                return response;
            }

            var result = await context.Bids.Where(x => x.VehicleId == model.VehicleId && x.Vehicle.IsActive == true).OrderByDescending(x => x.BidAmount).ToListAsync();
            if (result.Count == 0)
            {
                response.isSuccess = false;
                return response;
            }
            var objDTO = mapper.Map<Bid>(model);
            objDTO.BidAmount = result[0].BidAmount + (result[0].BidAmount * 10) / 100;
            objDTO.BidDate = DateTime.Now;
            context.Bids.Add(objDTO);
            await context.SaveChangesAsync();
            response.isSuccess = true;
            response.Result = result;
            return response;
        }

        public Task<ApiResponse> CancelBid(int bidId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse> CreateBid(CreateBidDTO model)
        {
            var returnValue = await CheckIsActive(model.VehicleId);
            var isPaid = await CheckIsPaidAuction(model.UserId, model.VehicleId);
            if (!isPaid)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add("Devam etmeden önce açık artırma ücretini ödemeniz gerekiyor.");
                return response;
            }
            if (returnValue == null)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add("Bu araç aktif değil.");
                return response;

            }
            if (returnValue.Price >= model.BidAmount)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add($"Bu araç için teklifiniz {returnValue.Price} değerinin üzerinde olmalıdır.");
                return response;
            }
            if (model != null)
            {
                var topPrice = await context.Bids.Where(x => x.VehicleId == model.VehicleId).OrderByDescending(x => x.BidAmount).ToListAsync();
                if (topPrice.Count != 0)
                {
                    if (topPrice[0].BidAmount >= model.BidAmount && model.BidAmount < topPrice[0].BidAmount + (topPrice[0].BidAmount * 1) / 100)
                    {
                        response.isSuccess = false;
                        response.ErrorMessages.Add(
                            $"Teklifiniz, sistemdeki en yüksek tekliften düşük olamaz. En yüksek teklif: {topPrice[0].BidAmount}"
                        );
                        return response;
                    }
                }
                Bid bid = mapper.Map<Bid>(model);
                bid.BidDate = DateTime.Now;
                await context.Bids.AddAsync(bid);
                if (await context.SaveChangesAsync() > 0)
                {
                    var userDetail = await context.Bids.Include(x => x.User).Where(x => x.UserId == model.UserId).FirstOrDefaultAsync();
                    _mailService.SendEmail("Teklif Başarılı",$"Teklif Tutarınız: {bid.BidAmount}",bid.User.UserName);
                    response.isSuccess = true;
                    response.Result = model;
                    return response;
                }

            }
            response.isSuccess = false;
            response.ErrorMessages.Add("Bir hata oluştu");
            return response;
        }

        public async Task<ApiResponse> GetBidById(int bidId)
        {
            var result = await context.Bids.Include(x => x.User).Where(x => x.BidId == bidId).FirstOrDefaultAsync();
            if (result == null)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add("bid is not found");
                return response;
            }

            response.isSuccess = true;
            response.Result = result;
            return response;


        }

        public async Task<ApiResponse> GetBidByVehicleId(int vehicleId)
        {
            var obj = await context.Bids.Include(x => x.Vehicle).ThenInclude(x => x.Bids).Where(x => x.VehicleId == vehicleId).ToListAsync();
            if (obj != null)
            {
                response.isSuccess = true;
                response.Result = obj;
                return response;
            }
            response.isSuccess = false;
            return response;
        }

        public async Task<ApiResponse> UpdateBid(int bidId, UpdateBidDTO model)
        {
            //Update eden kullanıcı en son verdiği teklifin üzerine çıkmalıdır.
            var isPaid = await CheckIsPaidAuction(model.UserId, model.VehicleId);
            if (!isPaid)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add("Lütfen önce açık artırma ücretini ödeyin.");
                return response;
            }
            var result = await context.Bids.FindAsync(bidId);
            if (result == null)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add("Teklif bulunamadı.");
                return response;
            }
            if (result.BidAmount < model.BidAmount && result.UserId == model.UserId)
            {
                var objDTO = mapper.Map(model, result);
                objDTO.BidDate = DateTime.Now;
                response.isSuccess = true;
                response.Result = objDTO;
                await context.SaveChangesAsync();
                return response;
            }
            else if (result.BidAmount >= model.BidAmount)
            {
                response.isSuccess = false;
                response.ErrorMessages.Add("You are not entry low price than your old bid amount,your older bid amount is : " + result.BidAmount);
                return response;
            }
            response.isSuccess = false;
            response.ErrorMessages.Add("Something went wrong");
            return response;

        }

        private async Task<Vehicle> CheckIsActive(int vehicleId)
        {
            var obj = await context.Vehicles.Where(x => x.VehicleId == vehicleId && x.IsActive == true && x.EndTime >= DateTime.Now).FirstOrDefaultAsync();
            if (obj != null)
            {
                return obj;
            }
            return null;
        }

        private async Task<bool> CheckIsPaidAuction(string userId, int vehicleId)
        {
            var obj = await context.PaymentHistories.Where(x => x.UserId == userId && x.VehicleId == vehicleId && x.IsActive == true).FirstOrDefaultAsync();
            if (obj != null)
            {
                return true;
            }
            return false;
        }



    }
}
