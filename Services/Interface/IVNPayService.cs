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
        Task<ApiResponse<VNPayCreateResponse>> CreatePaymentUrlAsync(Guid orderId, VNPayCreateRequest request, CancellationToken ct = default);
        Task<ApiResponse<VNPayReturnResponse>> HandleReturnAsync(IQueryCollection query, CancellationToken ct = default);
        Task<string> HandleIpnAsync(IQueryCollection query, CancellationToken ct = default); // "OK"/"INVALID"
    }
}
