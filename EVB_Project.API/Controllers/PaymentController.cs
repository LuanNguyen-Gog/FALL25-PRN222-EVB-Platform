using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    [Authorize(Roles = "member")]
    public class PaymentsController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        public PaymentsController(IVNPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        // Tạo payment URL theo Order (orderId ở route)
        [HttpPost("orders/{orderId:guid}/payments/vnpay")]
        public async Task<ActionResult<ApiResponse<VNPayCreateResponse>>> CreateForOrder(
            [FromRoute] Guid orderId,
            [FromBody] VNPayCreateRequest request,
            CancellationToken ct)
        {
            request.ClientIp ??= HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _vnPayService.CreatePaymentUrlAsync(orderId, request, ct);
            return Ok(res);
        }

        [HttpGet("payments/vnpay/return")]
        public async Task<ActionResult<ApiResponse<VNPayReturnResponse>>> Return(CancellationToken ct)
        {
            var res = await _vnPayService.HandleReturnAsync(Request.Query, ct);
            return Ok(res);
        }

        [HttpGet("payments/vnpay/ipn")]
        public async Task<IActionResult> Ipn(CancellationToken ct)
        {
            var text = await _vnPayService.HandleIpnAsync(Request.Query, ct);
            return Content(text, "text/plain");
        }
    }
}