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
        // lấy chi tiết kèm navigation
        public async Task<Order?> GetDetailAsync(Guid orderId, bool asNoTracking = true, CancellationToken ct = default)
        {
            var q = _context.Orders
                            .Include(o => o.Contract)
                            .Include(o => o.Payment)
                            .AsQueryable();
            if (asNoTracking) q = q.AsNoTracking();
            return await q.FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }

        // search + paging + filter
        public async Task<(IReadOnlyList<Order> Items, int Total)> SearchAsync(
            Guid? buyerId, OrderStatus? status, int page, int pageSize, CancellationToken ct = default)
        {
            var q = _context.Orders.AsQueryable();
            if (buyerId.HasValue) q = q.Where(o => o.BuyerId == buyerId.Value);
            if (status.HasValue) q = q.Where(o => o.Status == status.Value);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(o => o.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);
            return (items, total);
        }

        // validate Listing tồn tại
        public async Task<bool> ListingExistsAsync(Guid listingId, CancellationToken ct = default)
            => await _context.Set<Listing>().AnyAsync(x => x.Id == listingId, ct);

        public async Task<decimal?> GetPrice(Guid orderId, CancellationToken ct = default)
        {
            var order = await _context.Orders
        .AsNoTracking()
        .Where(o => o.Id == orderId)
        .Select(o => new { o.BatteryId, o.VehicleId })
        .FirstOrDefaultAsync(ct);

            if (order == null) return null;

            if (order.BatteryId is Guid bId)
                return await _context.Batteries
                    .AsNoTracking()
                    .Where(b => b.Id == bId)
                    .Select(b => (decimal?)b.PriceVnd)
                    .FirstOrDefaultAsync(ct);

            if (order.VehicleId is Guid vId)
                return await _context.Vehicles
                    .AsNoTracking()
                    .Where(v => v.Id == vId)
                    .Select(v => (decimal?)v.PriceVnd)
                    .FirstOrDefaultAsync(ct);

            return null;
        }

        public async Task<(string? BuyerEmail, string? SellerEmail)?> GetEmailsAsync(Guid orderId, CancellationToken ct = default)
        {
            var result = await _context.Orders
                .Where(o => o.Id == orderId)
                .Select(o => new
                {
                    BuyerEmail = o.Buyer != null ? o.Buyer.Email : null,
                    SellerEmail = o.Listing != null && o.Listing.Seller != null ? o.Listing.Seller.Email : null
                })
                .FirstOrDefaultAsync(ct);

            if (result == null)
                return null;

            return (result.BuyerEmail, result.SellerEmail);
        }

    }
}
