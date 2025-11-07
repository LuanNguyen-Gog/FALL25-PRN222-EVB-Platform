using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [ApiController]
    [Route("api/avatars")]
    public class AvatarsController : ControllerBase
    {
        private readonly IAvatarService _svc;

        public AvatarsController(IAvatarService svc) => _svc = svc;

        [HttpPost("users/{userId:guid}")]
        public async Task<IActionResult> UploadUser(Guid userId, IFormFile file, CancellationToken ct)
        {
            var url = await _svc.UploadUserAvatarAsync(userId, file, ct);
            return Ok(new { url });
        }

        [HttpPost("vehicles/{vehicleId:guid}")]
        public async Task<IActionResult> UploadVehicle(Guid vehicleId, IFormFile file, CancellationToken ct)
        {
            var url = await _svc.UploadVehicleAvatarAsync(vehicleId, file, ct);
            return Ok(new { url });
        }

        [HttpPost("batteries/{batteryId:guid}")]
        public async Task<IActionResult> UploadBattery(Guid batteryId, IFormFile file, CancellationToken ct)
        {
            var url = await _svc.UploadBatteryAvatarAsync(batteryId, file, ct);
            return Ok(new { url });
        }
    }
}