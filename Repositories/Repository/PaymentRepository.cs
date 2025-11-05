using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        public async Task UpsertPendingAsync(Guid orderId, decimal amountVnd)
        {
            var p = await _context.Set<Payment>().FirstOrDefaultAsync(x => x.OrderId == orderId);
            if (p == null)
            {
                p = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    AmountVnd = amountVnd,
                    Method = PaymentMethod.VnPay,
                    Status = PaymentStatus.Pending,
                    ProviderTxnId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.Set<Payment>().AddAsync(p);
            }
            else
            {
                p.AmountVnd = amountVnd;
                p.Method = PaymentMethod.VnPay;
                p.Status = PaymentStatus.Pending;
                p.ProviderTxnId = null;
                p.UpdatedAt = DateTime.UtcNow;
                _context.Set<Payment>().Update(p);
            }
            await _context.SaveChangesAsync(); // ❗️Đừng quên
        }

    }
}

