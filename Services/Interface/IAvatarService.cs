using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IAvatarService
    {
        Task<string> UploadUserAvatarAsync(Guid userId, IFormFile file, CancellationToken ct = default);
        Task<string> UploadVehicleAvatarAsync(Guid vehicleId, IFormFile file, CancellationToken ct = default);
        Task<string> UploadBatteryAvatarAsync(Guid batteryId, IFormFile file, CancellationToken ct = default);
    }
}
