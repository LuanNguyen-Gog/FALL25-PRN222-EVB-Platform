using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implement
{
    public class BatteryService : IBatteryService
    {
        private readonly BatteryRepository _batteryRepository;

        public BatteryService(BatteryRepository batteryRepository)
        {
            _batteryRepository = batteryRepository;
        }

        public async Task<PageResponse<BatteryResponse>> GetAllBatteries(BatteryFilterRequest request, int page, int pageSize)
        {
            var entity = request.Adapt<Battery>();
            var query = _batteryRepository.GetFiltered(entity);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PageResponse<BatteryResponse>
            {
                Items = items.Adapt<List<BatteryResponse>>(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<BatteryResponse>> GetBatteryById(Guid batteryId)
        {
            var battery = await _batteryRepository.GetByIdAsync(batteryId);
            if (battery == null)
            {
                return new ApiResponse<BatteryResponse>
                {
                    Success = false,
                    Message = "Battery not found"
                };
            }

            return new ApiResponse<BatteryResponse>
            {
                Success = true,
                Data = battery.Adapt<BatteryResponse>()
            };
        }

        public async Task<ApiResponse<BatteryResponse>> CreateBattery(BatteryCreateRequest request)
        {
            var battery = request.Adapt<Battery>();
            battery.Id = Guid.NewGuid();
            battery.CreatedAt = DateTime.UtcNow;
            battery.UpdatedAt = DateTime.UtcNow;

            await _batteryRepository.CreateAsync(battery);

            return new ApiResponse<BatteryResponse>
            {
                Success = true,
                Data = battery.Adapt<BatteryResponse>()
            };
        }

        public async Task<ApiResponse<bool>> UpdateBattery(Guid batteryId, BatteryUpdateRequest request)
        {
            var battery = await _batteryRepository.GetByIdAsync(batteryId);
            if (battery == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Battery not found"
                };
            }

            request.Adapt(battery);
            battery.UpdatedAt = DateTime.UtcNow;

            await _batteryRepository.UpdateAsync(battery);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> DeleteBattery(Guid batteryId)
        {
            var battery = await _batteryRepository.GetByIdAsync(batteryId);
            if (battery == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Battery not found"
                };
            }

            await _batteryRepository.RemoveAsync(battery);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}