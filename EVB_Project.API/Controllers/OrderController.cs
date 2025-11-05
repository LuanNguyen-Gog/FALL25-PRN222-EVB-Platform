using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("search")]
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
    }
}
