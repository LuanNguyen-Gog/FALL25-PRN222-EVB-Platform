using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [Route("api/contract")]
    [ApiController]
    [Authorize(Roles = "member")]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _svc;
        public ContractController(IContractService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContractResponse>>> Upsert(Guid orderId, [FromBody] ContractRequest req, CancellationToken ct)
        {
            var res = await _svc.UpsertForOrderAsync(orderId, req.ContractFileUrl, ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpPatch("sign")]
        public async Task<ActionResult<ApiResponse<ContractResponse>>> Sign(Guid orderId, CancellationToken ct)
        {
            var res = await _svc.SignAsync(orderId, ct);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<ContractResponse>>> Get(Guid orderId, CancellationToken ct)
        {
            var res = await _svc.GetByOrderAsync(orderId, ct);
            return res.Success ? Ok(res) : NotFound(res);
        }
    }
}
