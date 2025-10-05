using EVBTradingContract.Common;
using EVBTradingContract.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [Route("api/vehicles")]
    [ApiController]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<ActionResult<PageResponse<Vehicle>>> GetFiltered([FromQuery] VehicleFilterRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {

            var result = await _vehicleService.GetAllVehicle(requestDto, page, pageSize);
            return Ok(result);
        }
    }
}
