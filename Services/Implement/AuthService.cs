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
        private readonly RefreshTokenService _rtService;
        public AuthService(AuthRepository authRepo, RefreshTokenService rt)
        {
            _authRepo = authRepo;
            _rtService = rt;
        }

        public async Task<ApiResponse<AuthResponse>> Login(AuthRequest request, CancellationToken c = default)
        {
            try
            {
                var user = await _authRepo.GetUserByEmail(request.Email);
                if (user is null)
                    return new ApiResponse<AuthResponse>() 
                    { 
                        Success = false, 
                        Message = "You don't have an account!!!", 
                        Data = null 
                    };

                // TODO: Thay bằng verify password hash + salt
                var stored = (user.PasswordHash ?? string.Empty).Trim();
                var entered = (request.Password ?? string.Empty).Trim();
                if (!string.Equals(entered, stored, StringComparison.Ordinal))
                    return new ApiResponse<AuthResponse>()
                    {
                        Success = false,
                        Message = "Invalid email or password!!!",
                        Data = null
                    };

                var (access, expUtc) = _rtService.GenerateAccessToken(user, DateTime.UtcNow);
                var (plainRefresh, _) = await _rtService.IssueAsync(user, c);

                var tokenDto = new TokenResponse
                {
                    AccessToken = access,
                    RefreshToken = plainRefresh,
                    AccessTokenExpiresAtUtc = expUtc
                };

                return Ok(new AuthResponse
                {
                    token = tokenDto,
                    User = user.Adapt<UserResponse>()
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<TokenResponse>> Refresh(RefreshTokenRequest request, CancellationToken c = default)
        {
            try
            {
                var (newAccess, newRefresh, exp) = await _rtService.RotateAsync(request, c);
                return Ok(new TokenResponse
                {
                    AccessToken = newAccess,
                    RefreshToken = newRefresh,
                    AccessTokenExpiresAtUtc = exp
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> Logout(RefreshTokenRequest request, CancellationToken c = default)
        {
            try
            {
                await _rtService.RevokeAsync(request, "User logout", c);
                return new ApiResponse<bool>()
                {
                    Success = true,
                    Message = "User logout successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> RevokeAll(Guid userId, CancellationToken c = default)
        {
            try
            {
                var cnt = await _rtService.RevokeAllAsync(userId, "User revoke all", c);
                return new ApiResponse<bool>()
                {
                    Success = true,
                    Message = "User revoke successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
    }
}
