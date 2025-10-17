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
        public async Task AddRefreshTokenAsync(RefreshToken entity, CancellationToken ct = default)
        {
            await AddAsync(entity, ct);
            await SaveAsync(ct);
        }

        //public async Task<RefreshToken?> FindByHashAsync(string hash, CancellationToken ct = default)
        //{
        //    return await Query<RefreshToken>()
        //        .Include(x => x.User)
        //        .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        //}
    }
}
