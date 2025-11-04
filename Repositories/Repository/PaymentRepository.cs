using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repositories.Enum.Enum;

namespace Repositories.Repository
{
    public class PaymentRepository : GenericRepo<Payment>
    {
        private readonly EVBatteryTradingContext _context;
        public PaymentRepository(EVBatteryTradingContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByOrderIdAsync(Guid orderId, bool asNoTracking = true)
        {
            var q = _context.Payments.AsQueryable();
            if (asNoTracking) q = q.AsNoTracking();
            return await q.FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<bool> ExistsByProviderTxnIdAsync(string providerTxnId)
        {
            return await _context.Payments.AnyAsync(p => p.ProviderTxnId == providerTxnId);
        }

        // Update “thô” – service sẽ set giá trị cụ thể
        public async Task UpdateAsync(Payment entity)
        {
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
