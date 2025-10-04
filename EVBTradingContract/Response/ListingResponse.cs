using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class ListingResponse
    {
        public long ListingId { get; set; }

        public long SellerId { get; set; }

        public long VehicleId { get; set; }

        public long BatteryId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal PriceVnd { get; set; }

        public decimal AiSuggestedPriceVnd { get; set; }

        public string Status { get; set; }

        public long ApprovedBy { get; set; }

        public DateTime ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
