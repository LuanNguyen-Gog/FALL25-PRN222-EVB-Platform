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
    public class VehicleService : IVehicleService
    {
        private readonly VehicleRepository _vehicleRepository;

        public VehicleService(VehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<PageResponse<VehicleResponse>> GetAllVehicle(VehicleFilterRequest request, int page, int pageSize)
        {
            var entity = request.Adapt<Vehicle>();
            var query = _vehicleRepository.GetFiltered(entity);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PageResponse<VehicleResponse>
            {
                Items = items.Adapt<List<VehicleResponse>>(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<VehicleResponse>> GetVehicleById(Guid vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                return new ApiResponse<VehicleResponse>
                {
                    Success = false,
                    Message = "Vehicle not found"
                };
            }

            return new ApiResponse<VehicleResponse>
            {
                Success = true,
                Data = vehicle.Adapt<VehicleResponse>()
            };
        }

        public async Task<ApiResponse<VehicleResponse>> CreateVehicle(VehicleCreateRequest request)
        {
            var vehicle = request.Adapt<Vehicle>();
            vehicle.Id = Guid.NewGuid();
            vehicle.CreatedAt = DateTime.UtcNow;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _vehicleRepository.CreateAsync(vehicle);

            return new ApiResponse<VehicleResponse>
            {
                Success = true,
                Data = vehicle.Adapt<VehicleResponse>()
            };
        }

        public async Task<ApiResponse<bool>> UpdateVehicle(Guid vehicleId, VehicleUpdateRequest request)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Vehicle not found"
                };
            }

            request.Adapt(vehicle);
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _vehicleRepository.UpdateAsync(vehicle);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> DeleteVehicle(Guid vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Vehicle not found"
                };
            }

            await _vehicleRepository.RemoveAsync(vehicle);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}