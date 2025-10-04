using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBatteryService
    {
        Task<PageResponse<BatteryResponse>> GetAllBatteries(BatteryFilterRequest request, int page, int pageSize);
    }
}
