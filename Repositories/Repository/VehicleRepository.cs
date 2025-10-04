using Repositories.Basic;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class VehicleRepository : GenericRepo<Vehicle>
    {
        public VehicleRepository(DBContext.EVBatteryTradingContext context) : base(context)
        {
        }
        public IQueryable<Vehicle> GetFiltered(Vehicle vehicle)
        {
            var query = _context.Vehicles.AsQueryable();
            if (vehicle.VehicleId > 0)
                query = query.Where(v => v.VehicleId == vehicle.VehicleId);
            if (vehicle.OwnerId > 0)
                query = query.Where(v => v.OwnerId == vehicle.OwnerId);
            if (!string.IsNullOrEmpty(vehicle.Brand))
                query = query.Where(v => v.Brand == vehicle.Brand);
            if (!string.IsNullOrEmpty(vehicle.Model))
                query = query.Where(v => v.Model == vehicle.Model);
            if (vehicle.Year.HasValue)
                query = query.Where(v => v.Year == vehicle.Year);
            if (vehicle.OdometerKm.HasValue)
                query = query.Where(v => v.OdometerKm == vehicle.OdometerKm);
            if (!string.IsNullOrEmpty(vehicle.Status))
                query = query.Where(v => v.Status == vehicle.Status);
            return query.OrderBy(v => v.VehicleId);
            //cần phải .ToList() ở service để thực thi câu query
        }
    }
}
