using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [Route("api/vehicles")]
    [ApiController]
    [Authorize(Roles = "member")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<ActionResult<PageResponse<VehicleResponse>>> GetFiltered([FromQuery] VehicleFilterRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _vehicleService.GetAllVehicle(requestDto, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{vehicleId:guid}")]
        public async Task<ActionResult<ApiResponse<VehicleResponse>>> GetById([FromRoute] Guid vehicleId)
        {
            var result = await _vehicleService.GetVehicleById(vehicleId);
            if (result == null) return NotFound(new ApiResponse<VehicleResponse> { Success = false, Message = "Vehicle not found" });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<VehicleResponse>>> Create([FromBody] VehicleCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<VehicleResponse> { Success = false, Message = "Invalid payload" });

            var result = await _vehicleService.CreateVehicle(request);
            return CreatedAtAction(nameof(GetById), new { vehicleId = result.Data.VehicleId }, result);
        }

        [HttpPut("{vehicleId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update([FromRoute] Guid vehicleId, [FromBody] VehicleUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<bool> { Success = false, Message = "Invalid payload" });

            var result = await _vehicleService.UpdateVehicle(vehicleId, request);
            if (!result.Success)
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Vehicle not found" });

            return Ok(result);
        }

        [HttpDelete("{vehicleId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] Guid vehicleId)
        {
            var result = await _vehicleService.DeleteVehicle(vehicleId);
            if (!result.Success)
                return NotFound(new ApiResponse<bool> { Success = false, Message = "Vehicle not found" });

            return Ok(result);
        }
    }
}