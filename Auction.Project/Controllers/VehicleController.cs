using Auction.Business.Abstraction;
using Auction.Business.Dtos;
using Auction.Core.Models;
using Auction.DataAccess.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VehicleController(IVehicleService vehicleService, IWebHostEnvironment webHostEnvironment)
        {
            _vehicleService = vehicleService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("CreateVehicle")]
        public async Task<IActionResult> AddVehicle([FromForm] CreateVehicleDto model)
        {
            if (ModelState.IsValid)
            {
                if (model.File == null || model.File.Length == 0)
                {
                    return BadRequest();
                }
                string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "images");
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.File.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                model.Image = fileName;
                var result = await _vehicleService.CreateVehicle(model);
                if (result.isSuccess)
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.File.CopyToAsync(fileStream);
                    }
                    return Ok(result);
                }
            }
            return BadRequest();
        }
        [HttpGet("GetVehicle")]
        public async Task<IActionResult> GetAllVehicles()
        {
            ApiResponse vehicles = await _vehicleService.GetVehicle();
            return Ok(vehicles);
        }
        [HttpPut("UpdateVehicle")]
        public async Task<IActionResult> UpdateVehicle(int vehicleId, [FromBody] UpdateVehicleDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _vehicleService.UpdateVehicleResponse(vehicleId, model);
                if (result.isSuccess)
                {
                    return Ok(result);
                }
            }
            return BadRequest();
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{vehicleId}")]
        public async Task<IActionResult> DeleteVehicle([FromRoute] int vehicleId)
        {
            var result = await _vehicleService.GetVehicleById(vehicleId);
            if (result.isSuccess)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet("{vehicleId}")]
        public async Task<IActionResult> GetVehicleById([FromRoute] int vehicleId)
        {
            var result = await _vehicleService.GetVehicleById(vehicleId);
            if (result.isSuccess)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPut("{vehicleId}")]
        public async Task<IActionResult> ChangeVehivle([FromRoute] int vehicleId)
        {
            var result = await _vehicleService.ChangeVehivle(vehicleId);
            if (result.isSuccess)
            {
                return Ok(result);
            }
            return BadRequest();
        }
    }
}
