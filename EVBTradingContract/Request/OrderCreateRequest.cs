using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Request
{
    public class OrderCreateRequest
    {
        public Guid BuyerId { get; set; }
        public Guid ListingId { get; set; }
    }
}
