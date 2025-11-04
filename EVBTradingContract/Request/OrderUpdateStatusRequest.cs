using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Request
{
    public class OrderUpdateStatusRequest
    {
        public string Status { get; set; } = default!;
    }
}
