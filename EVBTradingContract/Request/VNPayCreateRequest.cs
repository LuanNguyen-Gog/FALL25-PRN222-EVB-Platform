using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Request
{
    public class VNPayCreateRequest
    {
        public string? BankCode { get; set; }   // "VNPAYQR", "VNBANK", "INTCARD", ...
        public string? ClientIp { get; set; }   // sẽ fallback từ HttpContext nếu null
    }
}
