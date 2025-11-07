using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Request
{
    public class ListingRequest
    {
    }
    public class ListingGetRequest
    {
        public Guid? ListingId { get; set; }

        public Guid? SellerId { get; set; }

        public Guid? VehicleId { get; set; }

        public Guid? BatteryId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public decimal? PriceVnd { get; set; }

        public string? Status { get; set; }

        public long? ApprovedBy { get; set; }
    }
}
