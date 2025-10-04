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
using System.Text;
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
            var item = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
             return new PageResponse<VehicleResponse>() {
                 Items = item.Adapt<List<VehicleResponse>>(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
        }
    }
}

