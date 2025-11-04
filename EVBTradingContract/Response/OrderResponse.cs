using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public Guid ListingId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
