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
            var item = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PageResponse<BatteryResponse>()
            {
                Items = item.Adapt<List<BatteryResponse>>(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
