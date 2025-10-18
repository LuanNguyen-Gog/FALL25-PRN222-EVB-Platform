using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Models;
using Repositories.Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implement
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _cfg;
        private readonly TokenRepository _repo;

        public RefreshTokenService(IConfiguration cfg, TokenRepository repo)
        {
            _cfg = cfg;
            _repo = repo;
        }
        public async Task<(string PlainToken, RefreshToken Entity)> IssueAsync(User user, CancellationToken c = default)
        {
            var plain = GenerateSecureToken();
            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = Hash(plain),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "7")),
                RevokedAt = null
            };

            await _repo.AddRefreshTokenAsync(entity, c); // Add + Save gọn nhất
            return (plain, entity);
        }

        public async Task<(string NewAccess, string NewRefresh, DateTime AccessExpUtc)> RotateAsync(string refreshTokenPlain, CancellationToken c = default)
        {
            var hash = Hash(refreshTokenPlain);
            var rt = await _repo.FindByHashAsync(hash, c);

            if (rt is null || rt.RevokedAt != null || rt.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            // revoke old
            rt.RevokedAt = DateTime.UtcNow;

            // Issue new refresh
            var (newPlainRt, u) = await IssueAsync(rt.User!, c);

            // New access
            var (access, expUtc) = GenerateAccessToken(rt.User!, DateTime.UtcNow);

            await _repo.SaveAsync();// lưu RevokedAt của token cũ
            return (access, newPlainRt, expUtc);
        }

        public async Task RevokeAsync(string refreshTokenPlain, string? reason = null, CancellationToken c = default)
        {
            var hash = Hash(refreshTokenPlain);
            var rt = await _repo.FindByHashAsync(hash, c);
            if (rt == null) return;
            rt.RevokedAt = DateTime.UtcNow;
            await _repo.SaveAsync();
        }

        public async Task<int> RevokeAllAsync(Guid userId, string? reason = null, CancellationToken c = default)
        {
            var list = await _repo.GetActiveByUserAsync(userId, c);

            foreach (var rt in list) rt.RevokedAt = DateTime.UtcNow;

            await _repo.SaveAsync();
            return list.Count;
        }
        public (string Token, DateTime ExpiresUtc) GenerateAccessToken(User user, DateTime nowUtc)
        {
            var key = _cfg["Jwt:Key"]!;
            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var minutes = int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "30");
            var expires = nowUtc.AddMinutes(minutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.Name ?? string.Empty),
                new(ClaimTypes.Role, user.Role ?? "User"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: nowUtc,
                expires: expires,
                signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return (token, expires);
        }
        // ==== Helpers (private) ====
        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
