using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class VNPayReturnResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? ProviderTxnId { get; set; }
        public string? RspCode { get; set; }
        public string? VnPayTxnNo { get; set; }
    }
}
