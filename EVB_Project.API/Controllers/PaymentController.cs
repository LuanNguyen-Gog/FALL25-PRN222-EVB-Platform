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
            // 1. Lấy URL FE + BE
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var backendUrl = _configuration["Backend:BaseUrl"] ?? "http://localhost:8080/api";

            // 2. Gọi OrderService để check IPN + đổi trạng thái + tạo contract draft
            var result = await _orderService.ConfirmPaymentSuccessAsync(Request.Query, ct);

            // 3. Lấy orderId để build contractDownloadUrl
            var orderIdStr = result.Data?.Order?.Id.ToString() ?? Request.Query["vnp_OrderInfo"].ToString();
            string? contractDownloadUrl = null;
            if (Guid.TryParse(orderIdStr, out var oid))
                contractDownloadUrl = $"{backendUrl}/orders/{oid}/contracts/download";

            // 4. Gộp params redirect về FE
            var queryParams = new Dictionary<string, string?>
            {
                ["success"] = result.Success.ToString().ToLowerInvariant(),
                ["message"] = result.Message,
                ["orderId"] = orderIdStr,
                ["contractDownloadUrl"] = contractDownloadUrl
            };

            var queryString = string.Join("&", queryParams
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));

            // 5. Redirect về FE (giữ nguyên /payment/return)
            var redirectUrl = $"{frontendUrl}/payment/return";
            if (!string.IsNullOrEmpty(queryString))
                redirectUrl += $"?{queryString}";

            return Redirect(redirectUrl);
        }
    }
}