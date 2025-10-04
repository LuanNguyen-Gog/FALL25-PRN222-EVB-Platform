using EVBTradingContract.Common;
using EVBTradingContract.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [Route("api/batteries")]
    [ApiController]
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
    }
}
