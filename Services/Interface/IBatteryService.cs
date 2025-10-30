using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using System;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBatteryService
    {
        Task<PageResponse<BatteryResponse>> GetAllBatteries(BatteryFilterRequest request, int page, int pageSize);
        Task<ApiResponse<BatteryResponse>> GetBatteryById(Guid batteryId);
        Task<ApiResponse<BatteryResponse>> CreateBattery(BatteryCreateRequest request);
        Task<ApiResponse<bool>> UpdateBattery(Guid batteryId, BatteryUpdateRequest request);
        Task<ApiResponse<bool>> DeleteBattery(Guid batteryId);
    }
}