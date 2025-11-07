using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repositories.Enum.Enum;

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
        public ContractStatus Status { get; set; }
    }
}
