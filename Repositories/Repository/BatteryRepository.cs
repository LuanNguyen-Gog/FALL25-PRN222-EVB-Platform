using Repositories.Basic;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class BatteryRepository : GenericRepo<Battery>
    {
        public BatteryRepository(DBContext.EVBatteryTradingContext context) : base(context)
        {
        }
        public IQueryable<Battery> GetFiltered(Battery battery)
        {
            var query = _context.Batteries.AsQueryable();
            if (battery.Id != Guid.Empty)
                query = query.Where(b => b.Id == battery.Id);
            if (battery.Id != Guid.Empty)
                query = query.Where(b => b.OwnerId == battery.OwnerId);
            if (!string.IsNullOrEmpty(battery.Brand))
                query = query.Where(b => b.Brand == battery.Brand);
            if (!string.IsNullOrEmpty(battery.Model))
                query = query.Where(b => b.Model == battery.Model);
            if (battery.BatteryCapacityKwh.HasValue)
                query = query.Where(b => b.BatteryCapacityKwh == battery.BatteryCapacityKwh);
            if (battery.BatteryHealthPct.HasValue)
                query = query.Where(b => b.BatteryHealthPct == battery.BatteryHealthPct);
            if (battery.CycleCount.HasValue)
                query = query.Where(b => b.CycleCount == battery.CycleCount);
            if (!string.IsNullOrEmpty(battery.Chemistry))
                query = query.Where(b => b.Chemistry == battery.Chemistry);
            if (battery.NominalVoltageV.HasValue)
                query = query.Where(b => b.NominalVoltageV == battery.NominalVoltageV);
            if (!string.IsNullOrEmpty(battery.CompatibilityNote))
                query = query.Where(b => b.CompatibilityNote == battery.CompatibilityNote);
            if (battery.Status.HasValue)
                query = query.Where(b => b.Status == battery.Status);
            return query.OrderBy(b => b.Id);
            //cần phải .ToList() ở service để thực thi câu query
        }
    }
}
