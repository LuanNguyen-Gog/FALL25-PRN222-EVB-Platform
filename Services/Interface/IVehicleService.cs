using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using System;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IVehicleService
    {
        Task<PageResponse<VehicleResponse>> GetAllVehicle(VehicleFilterRequest request, int page, int pageSize);
        Task<ApiResponse<VehicleResponse>> GetVehicleById(Guid vehicleId);
        Task<ApiResponse<VehicleResponse>> CreateVehicle(VehicleCreateRequest request);
        Task<ApiResponse<bool>> UpdateVehicle(Guid vehicleId, VehicleUpdateRequest request);
        Task<ApiResponse<bool>> DeleteVehicle(Guid vehicleId);
    }
}
