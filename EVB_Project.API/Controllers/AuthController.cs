using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Services.Implement;
using Services.Interface;
using System.IdentityModel.Tokens.Jwt;
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
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<AuthResponse> { Success = false, Message = "Invalid payload", Data = null });

            var response = await _auth.Login(request);
            if (!response.Success)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken c)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<TokenResponse> { Success = false, Message = "Invalid payload", Data = null });
            var response = await _auth.Refresh(request, c);
            if (!response.Success)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
        [HttpPost("logout")]
        [AllowAnonymous] // logout chỉ cần refresh token
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RefreshTokenRequest request, CancellationToken c)
        {
            var response = await _auth.Logout(request, c);
            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }
        [HttpPost("revoke-all")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeAll(CancellationToken c)
        {
            // Lấy userId từ JWT: ưu tiên 'sub', fallback 'nameidentifier'
            var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid token or missing user id",
                    Data = false
                });
            }
            var response = await _auth.RevokeAll(userId, c);
            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] EVBTradingContract.Request.RegisterRequest request, CancellationToken c)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid payload",
                    Data = null
                });

            var result = await _auth.Register(request, c);
            if (!result.Success)
            {
                // nếu service trả về lỗi "Email already exists" → dùng 409
                return StatusCode(StatusCodes.Status409Conflict, result);
            }

            // Có thể trả 201 Created nếu bạn có endpoint GetUserById; tạm thời trả 200
            return Ok(result);
        }
    }
}
