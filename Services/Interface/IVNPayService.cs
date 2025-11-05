using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IVNPayService
    {
        Task<ApiResponse<VNPayCreateResponse>> GetPaymentUrl(Guid orderId);
        Task<ApiResponse<VNPayReturnResponse>> ProcessIpnAction(IQueryCollection query);
    }
}
