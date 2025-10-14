using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly AuthRepository _authRepo;
        private readonly IConfiguration _cfg;
        public AuthService(AuthRepository authRepo, IConfiguration cfg)
        {
            _authRepo = authRepo;
            _cfg = cfg;
        }
        public async Task<ApiResponse<AuthResponse>> Login(AuthRequest request)
        {
            try
            {
                //var user = await _authRepo.GetUserByEmail(request.Email);
                //if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                //    return new ApiResponse<AuthResponse>()
                //    {
                //        Success = false,
                //        Message = "Invalid email or password",
                //        Data = null
                //    };
                //if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
                //    return new ApiResponse<AuthResponse>()
                //    {
                //        Success = false,
                //        Message = "User is not active",
                //        Data = null
                //    };
                var user = await _authRepo.GetUserByEmail(request.Email);
                if (user == null)
                    return new ApiResponse<AuthResponse>()
                    {
                        Success = false,
                        Message = "You don't have an account!!!",
                        Data = null
                    };

                var stored = (user.PasswordHash ?? string.Empty).Trim();
                var entered = (request.Password ?? string.Empty).Trim();
                var role = (user.Role ?? string.Empty).Trim();

                if (!string.Equals(entered, stored, StringComparison.Ordinal))
                    return new ApiResponse<AuthResponse>()
                    {
                        Success = false,
                        Message = "Invalid email or password!!!",
                        Data = null
                    };

                //Lấy cấu hình JWT
                var key = _cfg["Jwt:Key"]!;
                var issuer = _cfg["Jwt:Issuer"]!;
                var audience = _cfg["Jwt:Audience"]!;
                var minutes = int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "30");
                var expires = DateTime.UtcNow.AddMinutes(minutes);

                //Khai báo claims
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                //Tạo signing credentials
                var creds = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256);

                //Sinh JWT
                var jwt = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: expires,
                    signingCredentials: creds);

                var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                //Map entity User -> UserResponse (Mapster)
                var userLogin = user.Adapt<UserResponse>();

                return new ApiResponse<AuthResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new AuthResponse
                    {
                        AccessToken = token,
                        ExpiresAtUtc = expires,
                        User = userLogin
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AuthResponse>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
