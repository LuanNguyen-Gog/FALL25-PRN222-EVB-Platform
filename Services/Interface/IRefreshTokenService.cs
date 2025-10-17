using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IRefreshTokenService
    {
        (string Token, DateTime ExpiresUtc) GenerateAccessToken(User user, DateTime nowUtc);
        Task<(string PlainToken, RefreshToken Entity)> IssueAsync(User user, CancellationToken c = default);
        Task<(string NewAccess, string NewRefresh, DateTime AccessExpUtc)> RotateAsync(string refreshTokenPlain, CancellationToken c = default);
        Task RevokeAsync(string refreshTokenPlain, string? reason = null, CancellationToken c = default);
        Task<int> RevokeAllAsync(Guid userId, string? reason = null, CancellationToken c = default);
    }
}
