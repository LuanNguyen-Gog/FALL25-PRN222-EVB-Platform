using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Enum
{
    public class Enum
    {
        public enum UserStatus { Active, Inactive }
        public enum AssetStatus { Available, Reserved, Sold, Archived }
        public enum ListingStatus { Draft, Pending, Active, Rejected, Sold, Archived }
        public enum OrderStatus { Pending, Processing, Completed, Cancelled }
        public enum PaymentStatus { Pending, Success, Failed, Refunded }
        public enum ComplaintStatus { Open, InProgress, Resolved, Rejected }
    }
}
