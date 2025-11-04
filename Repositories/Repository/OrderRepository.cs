using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class OrderRepository : GenericRepo<Order>
    {
        private readonly EVBatteryTradingContext context;
        public OrderRepository(EVBatteryTradingContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Order?> GetByIdAsync(Guid orderId, bool asNoTracking = true)
        {
            var q = _context.Orders.AsQueryable();
            if (asNoTracking) q = q.AsNoTracking();
            return await q.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdateAsync(Order entity)
        {
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
