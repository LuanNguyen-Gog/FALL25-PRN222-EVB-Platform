using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class VNPayReturnResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? OrderId { get; set; }
        public string? Amount { get; set; }             // vnp_Amount (chuỗi gốc)
        public string? TransactionNo { get; set; }      // vnp_TransactionNo
        public string? ResponseCode { get; set; }       // vnp_ResponseCode
        public string? TransactionStatus { get; set; }  // vnp_TransactionStatus
    }
}
