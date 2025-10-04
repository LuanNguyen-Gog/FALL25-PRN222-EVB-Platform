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
        public long? ListingId { get; set; }

        public long? SellerId { get; set; }

        public long? VehicleId { get; set; }

        public long? BatteryId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public decimal? PriceVnd { get; set; }

        public decimal? AiSuggestedPriceVnd { get; set; }

        public string? Status { get; set; }

        public long? ApprovedBy { get; set; }
    }
}
