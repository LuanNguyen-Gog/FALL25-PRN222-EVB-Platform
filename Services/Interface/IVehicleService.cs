using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IVehicleService
    {
        Task<PageResponse<VehicleResponse>> GetAllVehicle(VehicleFilterRequest request, int page, int pageSize);
        Task<VehicleResponse> GetVehicleByIdAsync(Guid vehicleId);
        Task<VehicleResponse> CreateVehicleAsync(VehicleRequest request);
        Task<bool> UpdateVehicleAsync(Guid vehicleId, VehicleRequest request);
        Task<bool> DeleteVehicleAsync(Guid vehicleId);
    }
}
