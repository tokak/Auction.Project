using Auction.Business.Abstraction;
using Auction.Business.Dtos;
using Auction.Core.Models;
using Auction.DataAccess.Context;
using Auction.DataAccess.Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Auction.Business.Concrete
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _contex;
        private readonly IMapper _mapper;
        private ApiResponse _apiResponse;
        public VehicleService(ApplicationDbContext contex, IMapper mapper, ApiResponse apiResponse)
        {
            _contex = contex;
            _mapper = mapper;
            _apiResponse = apiResponse;
        }

        public async Task<ApiResponse> ChangeVehivle(int vehicleId)
        {
            var result = await _contex.Vehicles.FindAsync(vehicleId);
            if (result is not null)
            {
                result.IsActive = false;
                if (await _contex.SaveChangesAsync() > 0)
                    return _apiResponse;
            }
            _apiResponse.isSuccess = false;
            return _apiResponse;
        }

        public async Task<ApiResponse> CreateVehicle(CreateVehicleDto model)
        {
            if (model is not null)
            {
                var objDto = _mapper.Map<Vehicle>(model);
                if (objDto is not null)
                {
                    await _contex.Vehicles.AddAsync(objDto);
                    if (await _contex.SaveChangesAsync() > 0)
                    {
                        _apiResponse.isSuccess = true;
                        _apiResponse.Result = model;
                        _apiResponse.StatusCode = System.Net.HttpStatusCode.Created;
                        return _apiResponse;
                    }
                }
            }
            _apiResponse.isSuccess = false;
            _apiResponse.ErrorMessages.Add("Kayıt oluştururken hata oluştu.");
            return _apiResponse;

        }

        public async Task<ApiResponse> DeleteVehicle(int vehicleId)
        {
            var result = await _contex.Vehicles.FindAsync(vehicleId);
            if (result is not null)
            {
                _contex.Vehicles.Remove(result);
                if (await _contex.SaveChangesAsync() > 0)
                {
                    _apiResponse.isSuccess = true;
                    return _apiResponse;
                }
            }
            _apiResponse.isSuccess = false;
            return _apiResponse;
        }

        public async Task<ApiResponse> GetVehicle()
        {
            var vehicle = await _contex.Vehicles.Include(x => x.Seller).ToListAsync();
            if (vehicle is not null)
            {
                _apiResponse.isSuccess = true;
                _apiResponse.Result = vehicle;
                return _apiResponse;
            }
            _apiResponse.isSuccess = false;
            return _apiResponse;
        }

        public async Task<ApiResponse> GetVehicleById(int vehicleId)
        {
            var result = await _contex.Vehicles.Include(s => s.Seller).FirstOrDefaultAsync(x => x.VehicleId == vehicleId);
            if (result is not null)
            {
                _apiResponse.Result = result;
                _apiResponse.isSuccess = true;
                return _apiResponse;
            }
            _apiResponse.isSuccess = false;
            return _apiResponse;
        }

        public async Task<ApiResponse> UpdateVehicleResponse(int vehicleId, UpdateVehicleDto model)
        {
            var result = await _contex.Vehicles.FindAsync(vehicleId);
            if (result is not null)
            {
                Vehicle objDto = _mapper.Map(model, result);
                if (await _contex.SaveChangesAsync() > 0)
                {
                    _apiResponse.isSuccess = true;
                    _apiResponse.Result = objDto;
                    return _apiResponse;
                }
            }
            _apiResponse.isSuccess = false;
            return _apiResponse;
        }
    }
}
