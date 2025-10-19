using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class BatteryResponse
    {
        public Guid BatteryId { get; set; }

        public Guid OwnerId { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public decimal BatteryCapacityKwh { get; set; }

        public decimal BatteryHealthPct { get; set; }

        public int CycleCount { get; set; }

        public string Chemistry { get; set; }

        public decimal NominalVoltageV { get; set; }

        public string CompatibilityNote { get; set; }

        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
