using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class TokenRepository : GenericRepo<RefreshToken>
    {
        public TokenRepository(DBContext.EVBatteryTradingContext context) : base(context)
        {
        }
        // Thêm refresh token và lưu xuống DB
        public async Task AddRefreshTokenAsync(RefreshToken entity, CancellationToken ct = default)
        {
            await CreateAsync(entity);
        }
        // Lấy token theo hash (kèm User).
        public async Task<RefreshToken?> FindByHashAsync(string hash, CancellationToken ct = default)
        {
            return await _context.Set<RefreshToken>()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        }
        // Lấy danh sách token còn hiệu lực (chưa revoke, chưa hết hạn) theo UserId.
        public async Task<List<RefreshToken>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<RefreshToken>()
                .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
                .ToListAsync(ct);
        }
    }
}
