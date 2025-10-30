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
        public async Task<ActionResult<PageResponse<Battery>>> GetAllBattery([FromQuery] BatteryFilterRequest request, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _batteryService.GetAllBatteries(request, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{batteryId:guid}")]
        public async Task<ActionResult<ApiResponse<BatteryResponse>>> GetById([FromRoute] Guid batteryId)
        {
            var result = await _batteryService.GetBatteryById(batteryId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BatteryResponse>>> Create([FromBody] BatteryCreateRequest request)
        {
            var result = await _batteryService.CreateBattery(request);
            return Ok(result);
        }

        [HttpPut("{batteryId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update([FromRoute] Guid batteryId, [FromBody] BatteryUpdateRequest request)
        {
            var result = await _batteryService.UpdateBattery(batteryId, request);
            return Ok(result);
        }

        [HttpDelete("{batteryId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] Guid batteryId)
        {
            var result = await _batteryService.DeleteBattery(batteryId);
            return Ok(result);
        }
    }
}