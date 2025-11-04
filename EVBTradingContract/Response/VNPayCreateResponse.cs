using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class VNPayCreateResponse
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal AmountVnd { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
    }
}
