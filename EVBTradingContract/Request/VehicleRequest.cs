using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Request
{
    public class VehicleRequest
    {
    }
    public class VehicleFilterRequest
    {
        public long? VehicleId { get; set; }
        public long? OwnerId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public int? OdometerKm { get; set; }
        public string? Status { get; set; }
    }
}
