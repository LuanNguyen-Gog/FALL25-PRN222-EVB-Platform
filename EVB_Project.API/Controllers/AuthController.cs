using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Services.Implement;
using Services.Interface;
using System.Security.Claims;

namespace EVB_Project.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] AuthRequest request)
        {
            var response = await _auth.Login(request);
            if (!response.Success)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Refresh([FromBody] RefreshRequest request, CancellationToken c)
        {
            var response = await _auth.Refresh(request, c);
            if (!response.Success)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
        [HttpPost("logout")]
        [AllowAnonymous] // logout chỉ cần refresh token
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RefreshRequest request, CancellationToken ct)
        {
            var result = await _auth.Logout(request, ct);
            return Ok(result);
        }
        [HttpPost("revoke-all")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<int>>> RevokeAll(CancellationToken ct)
        {
            // Lấy userId từ AccessToken (JWT)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ApiResponse<int>
                {
                    Success = false,
                    Message = "Invalid token or missing user id",
                    Data = 0
                });
            var result = await _auth.RevokeAll(userId, ct);
            return Ok(result);
        }
}
