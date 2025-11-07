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
    [Route("api/orders")]
    //[Authorize(Roles = "member")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _svc;
        public OrderController(IOrderService svc)
        {
            _svc = svc;
        }
        public sealed record PaymentConfirmRequest(string? ProviderTxnId, decimal? AmountVnd);
        public sealed record ContractCancelRequest(string? Reason);


        [HttpPost("create-order")]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> Create([FromBody] OrderCreateRequest req, CancellationToken ct)
        {
            var res = await _svc.CreateAsync(req.BuyerId, req.ListingId, ct);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> Get(Guid id, CancellationToken ct)
        {
            var o = await _svc.GetAsync(id, ct);
            if (o == null) return NotFound();
            return Ok(o);
        }

        [HttpGet("{buyerId:guid}/search")]
        public async Task<ActionResult<ApiResponse<object>>> Search(
            [FromQuery] Guid? buyerId, [FromQuery] string? status,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var res = await _svc.SearchAsync(buyerId, status, page, pageSize, ct);
            return Ok(res);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateStatus(Guid id,
            [FromBody] OrderUpdateStatusRequest req, CancellationToken ct)
        {
            if (!Enum.TryParse<Repositories.Enum.Enum.OrderStatus>(req.Status, true, out var newStatus))
                return BadRequest();

            var o = await _svc.UpdateStatusAsync(id, newStatus, ct);
            return Ok(o);
        }
        /// <summary>
        /// VNPay báo thành công (hoặc FE test): giữ tiền (Success), tạo draft contract, Order -> Processing
        /// </summary>
        [HttpPost("{orderId:guid}/payment/success")]
        [AllowAnonymous] // để IPN hoặc FE test có thể gọi. Muốn khóa lại -> thay bằng [Authorize]
        public async Task<ActionResult<ApiResponse<OrderAndContractResponse>>> PaymentSuccess(
            Guid orderId, CancellationToken ct)
        {
            var res = await _svc.ConfirmPaymentSuccessAsync(orderId, ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Buyer click-to-accept hợp đồng: Order -> Completed, giữ nguyên payment = Success
        /// </summary>
        [HttpPost("{orderId:guid}/contract/accept")]
        [Authorize] // tuỳ bạn, có thể AllowAnonymous khi thử nghiệm
        public async Task<ActionResult<ApiResponse<OrderAndContractResponse>>> ContractAccept(
            Guid orderId,
            CancellationToken ct)
        {
            var res = await _svc.HandleContractAcceptedAsync(orderId, ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Hợp đồng thất bại/huỷ: Payment -> Refunded, Order -> Cancelled
        /// </summary>
        [HttpPost("{orderId:guid}/contract/cancel")]
        [Authorize] // tuỳ nhu cầu
        public async Task<ActionResult<ApiResponse<OrderAndContractResponse>>> ContractCancel(
            Guid orderId,
            [FromBody] ContractCancelRequest? req,
            CancellationToken ct)
        {
            var res = await _svc.HandleContractCancelledAsync(orderId, req?.Reason, ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }
    }
}