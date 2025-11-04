using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class ContractResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ContractFileUrl { get; set; } = default!;
        public DateTime? SignedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
