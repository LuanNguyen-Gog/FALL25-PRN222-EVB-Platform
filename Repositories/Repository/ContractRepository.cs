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
    public class ContractRepository : GenericRepo<Contract>
    {
        public ContractRepository(EVBatteryTradingContext context) : base(context)
        {
        }
        public async Task<Contract?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.OrderId == orderId, ct);
        }

        public async Task<Contract?> GetTrackingByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _context.Contracts
                .FirstOrDefaultAsync(c => c.OrderId == orderId, ct);
        }
    }
}