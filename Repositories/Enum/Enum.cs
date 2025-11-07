using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Enum
{
    public class Enum
    {
        public enum UserStatus { Active = 0, Inactive = 1}
        public enum AssetStatus { Available = 0, Reserved = 1, Sold = 2, Archived = 3}
        public enum ListingStatus 
        { 
            Draft = 0, 
            Pending = 1, 
            Active = 2, 
            Rejected = 3, 
            Sold = 4, 
            Archived = 5 
        }
        public enum OrderStatus 
        { 
            Pending = 0, 
            Processing = 1, 
            Completed = 2, 
            Cancelled = 3
        }
        public enum PaymentStatus { Pending = 0, Success = 1, Failed = 2, Refunded = 3 }
        public enum ComplaintStatus { Open = 0, InProgress = 1, Resolved = 2, Rejected = 3 }
        public enum PaymentMethod
        {
            VnPay = 0
        }
        public enum ContractStatus
        {
            Draft = 0,            // vừa được tạo sau khi thanh toán, chưa ai ký
            Active = 1,           // cả hai bên đã ký, có hiệu lực
            Cancelled = 2        // bị hủy (1 hoặc cả hai bên)      
        }

    }
}
