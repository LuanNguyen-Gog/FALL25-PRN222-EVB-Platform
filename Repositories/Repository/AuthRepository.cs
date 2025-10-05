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
    public class AuthRepository : GenericRepo<User>
    {
        public AuthRepository(DBContext.EVBatteryTradingContext context) : base(context)
        {
        }

        public async Task<User?> GetUserByEmail(string email, bool asNoTracking = true)
        {
            return await FirstOrDefaultAsync(u => u.Email == email, asNoTracking);
        }
        public async Task<bool> ExistEmail(string email)
        {
            return await _context.Set<User>().AnyAsync(u => u.Email == email);
        }
    }
}
