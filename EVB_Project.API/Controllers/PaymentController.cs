using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [ApiController]
    [Route("api/payments")] // số nhiều
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IVNPayService _vnPay;
        public PaymentsController(IVNPayService vnPay) => _vnPay = vnPay;

        // POST /api/orders/{orderId}/payments/vnpay
        // -> trả ApiResponse<VNPayCreateResponse> (chứa paymentUrl)
        [HttpPost("orders/{orderId:guid}/payments/vnpay")]
        public async Task<ActionResult<ApiResponse<VNPayCreateResponse>>> CreateVnPay(
            [FromRoute] Guid orderId)
        {
            var res = await _vnPay.GetPaymentUrl(orderId);
            return Ok(res);
        }

        // GET /api/payments/vnpay/callback?...
        // -> trả ApiResponse<VNPayReturnResponse> (kết quả từ VNPay)
        [HttpGet("payments/vnpay/callback")]
        public async Task<ActionResult<ApiResponse<VNPayReturnResponse>>> VnPayCallback()
        {
            var res = await _vnPay.ProcessIpnAction(Request.Query);
            return Ok(res);
        }
    }
}