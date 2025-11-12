using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Implement;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [ApiController]
    [Route("api/payments")] // số nhiều
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly IVNPayService _vnPay;
        public PaymentsController(IVNPayService vnPay, IConfiguration configuration, IOrderService orderService)
        {
            _vnPay = vnPay;
            _configuration = configuration;
            _orderService = orderService;
        }

        // POST /api/orders/{orderId}/payments/vnpay
        // -> trả ApiResponse<VNPayCreateResponse> (chứa paymentUrl)
        [HttpPost("orders/{orderId:guid}/payments/vnpay")]
        public async Task<ActionResult<ApiResponse<VNPayCreateResponse>>> CreateVnPay(
            [FromRoute] Guid orderId)
        {
            var res = await _vnPay.GetPaymentUrl(orderId);
            return Ok(res);
        }

        [HttpGet("vnpay/return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn(CancellationToken ct)
        {
            // 1️⃣ Lấy base URL cho FE và BE
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var backendUrl = _configuration["Backend:BaseUrl"] ?? "http://localhost:8080/api";

            // 2️⃣ Xử lý VNPay callback (tự gọi IPN ngay tại đây)
            var ipnResult = await _vnPay.ProcessIpnAction(Request.Query);

            // 3️⃣ Lấy orderId từ vnp_OrderInfo (VNPay gửi kèm)
            var orderIdStr = Request.Query["vnp_OrderInfo"].ToString();
            string? contractDownloadUrl = null;

            if (Guid.TryParse(orderIdStr, out var orderId))
            {
                // 4️⃣ Nếu IPN xử lý thành công, gọi confirm để tạo contract draft
                if (ipnResult.Success)
                {
                    await _orderService.ConfirmPaymentSuccessAsync(orderId, ct);
                    contractDownloadUrl = $"{backendUrl}/orders/{orderId}/contracts/download";
                }
            }

            // 5️⃣ Gộp các tham số để redirect về FE
            var queryParams = new Dictionary<string, string?>
            {
                ["success"] = ipnResult.Success.ToString().ToLowerInvariant(),
                ["message"] = ipnResult.Message,
                ["orderId"] = orderIdStr,
                ["contractDownloadUrl"] = contractDownloadUrl
            };

            var queryString = string.Join("&",
                queryParams.Where(kv => !string.IsNullOrEmpty(kv.Value))
                           .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));

            // 6️⃣ Redirect về FE (/payment/return)
            var redirectUrl = $"{frontendUrl}/payment/return";
            if (!string.IsNullOrEmpty(queryString))
                redirectUrl += $"?{queryString}";

            return Redirect(redirectUrl);
        }
    }
}