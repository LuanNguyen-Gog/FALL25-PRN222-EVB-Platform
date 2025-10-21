using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> Login(AuthRequest request, CancellationToken c = default);
        Task<ApiResponse<TokenResponse>> Refresh(RefreshTokenRequest request, CancellationToken c = default);
        Task<ApiResponse<bool>> Logout(RefreshTokenRequest request, CancellationToken c = default);
        Task<ApiResponse<bool>> RevokeAll(Guid userId, CancellationToken c = default);

        Task<ApiResponse<AuthResponse>> Register(RegisterRequest request, CancellationToken c = default);
    }
}
