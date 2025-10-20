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
    [Route("api/batteries")]
    [ApiController]
    [Authorize(Roles = "member")]
    public class BatteryController : ControllerBase
    {
        private readonly IBatteryService _batteryService;
        public BatteryController(IBatteryService batteryService)
        {
            _batteryService = batteryService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PageResponse<BatteryResponse>>>> GetAllBattery(
            [FromQuery] BatteryFilterRequest request,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _batteryService.GetAllBatteries(request, page, pageSize);
                return Ok(new ApiResponse<PageResponse<BatteryResponse>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<PageResponse<BatteryResponse>>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpGet("{batteryId:guid}")]
        public async Task<ActionResult<ApiResponse<BatteryResponse>>> GetBatteryById([FromRoute] Guid batteryId)
        {
            try
            {
                var result = await _batteryService.GetBatteryByIdAsync(batteryId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<BatteryResponse>
                    {
                        Success = false,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<BatteryResponse>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<BatteryResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BatteryResponse>>> CreateBattery([FromBody] BatteryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<BatteryResponse>
                {
                    Success = false,
                    Data = null
                });
            }

            try
            {
                var result = await _batteryService.CreateBatteryAsync(request); 
                return Ok(new ApiResponse<BatteryResponse>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<BatteryResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpPut("{batteryId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateBattery([FromRoute] Guid batteryId, [FromBody] BatteryRequest request)
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
                var result = await _batteryService.UpdateBatteryAsync(batteryId, request);
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

        [HttpDelete("{batteryId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBattery([FromRoute] Guid batteryId)
        {
            try
            {
                var result = await _batteryService.DeleteBatteryAsync(batteryId);
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