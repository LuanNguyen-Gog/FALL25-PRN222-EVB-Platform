using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;
using System.Net;
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
        public async Task<ActionResult<ApiResponse<PageResponse<VehicleResponse>>>> GetFiltered(
            [FromQuery] VehicleFilterRequest requestDto,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _vehicleService.GetAllVehicle(requestDto, page, pageSize);
                return Ok(new ApiResponse<PageResponse<VehicleResponse>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<PageResponse<VehicleResponse>>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpGet("{vehicleId:guid}")]
        public async Task<ActionResult<ApiResponse<VehicleResponse>>> GetById([FromRoute] Guid vehicleId)
        {
            try
            {
                var result = await _vehicleService.GetVehicleByIdAsync(vehicleId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<VehicleResponse>
                    {
                        Success = false,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<VehicleResponse>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<VehicleResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<VehicleResponse>>> Create([FromBody] VehicleRequest requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<VehicleResponse>
                {
                    Success = false,
                    Data = null
                });
            }

            try
            {
                var result = await _vehicleService.CreateVehicleAsync(requestDto);
                return Ok(new ApiResponse<VehicleResponse>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<VehicleResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpPut("{vehicleId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update([FromRoute] Guid vehicleId, [FromBody] VehicleRequest requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Data = false
                });
            }

            try
            {
                var result = await _vehicleService.UpdateVehicleAsync(vehicleId, requestDto);
                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Data = false
                });
            }
        }

        [HttpDelete("{vehicleId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] Guid vehicleId)
        {
            try
            {
                var result = await _vehicleService.DeleteVehicleAsync(vehicleId);
                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Data = false
                });
            }
        }
    }
}

