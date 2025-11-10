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
        private readonly IConfiguration _configuration;
        private readonly IVNPayService _vnPay;
        public PaymentsController(IVNPayService vnPay, IConfiguration configuration)
        {
            _vnPay = vnPay;
            _configuration = configuration;
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

        // GET /api/payments/vnpay/callback?...
        // -> trả ApiResponse<VNPayReturnResponse> (kết quả từ VNPay)
        [HttpGet("payments/vnpay/callback")]
        public async Task<ActionResult<ApiResponse<VNPayReturnResponse>>> VnPayCallback()
        {
            var res = await _vnPay.ProcessIpnAction(Request.Query);
            return Ok(res);
        }

        // GET /api/payments/vnpay/return?...
        [HttpGet("vnpay/return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn(CancellationToken ct)
        {
            // 1) Base URLs
            var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var backendBase = _configuration["Backend:BaseUrl"] ?? "http://localhost:8080/api";

            // 2) Gọi service để xác thực & cập nhật trạng thái (Payment + Order)
            var ipn = await _vnPay.ProcessIpnAction(Request.Query); // cập nhật DB ở đây

            // 3) Lấy orderId (ưu tiên từ service; fallback từ query VNPay)
            var orderId = ipn.Data?.OrderId
                          ?? Request.Query["vnp_OrderInfo"].ToString()
                          ?? Request.Query["vnp_TxnRef"].ToString();

            // 4) Build link download hợp đồng (nếu orderId hợp lệ GUID)
            string? contractDownloadUrl = null;
            if (Guid.TryParse(orderId, out var oid))
                contractDownloadUrl = $"{backendBase}/orders/{oid}/contracts/download";

            // 5) Gom params trả về cho FE (FE sẽ đọc và hiển thị)
            var dict = new Dictionary<string, string?>
            {
                ["success"] = ipn.Success.ToString().ToLowerInvariant(),
                ["message"] = ipn.Message,
                ["orderId"] = orderId,
                ["amount"] = ipn.Data?.Amount ?? Request.Query["vnp_Amount"].ToString(),
                ["transactionNo"] = ipn.Data?.TransactionNo ?? Request.Query["vnp_TransactionNo"].ToString(),
                ["responseCode"] = ipn.Data?.ResponseCode ?? Request.Query["vnp_ResponseCode"].ToString(),
                ["transactionStatus"] = ipn.Data?.TransactionStatus ?? Request.Query["vnp_TransactionStatus"].ToString(),
                ["contractDownloadUrl"] = contractDownloadUrl
            };

            // Encode gọn gàng
            var qs = string.Join("&",
                dict.Where(kv => !string.IsNullOrEmpty(kv.Value))
                    .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));

            // 6) Redirect về FE
            var redirectUrl = $"{frontendBase}/payment/return";
            return Redirect(qs.Length > 0 ? $"{redirectUrl}?{qs}" : redirectUrl);
        }
    }
}