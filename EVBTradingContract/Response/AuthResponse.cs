using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public UserResponse User { get; set; }
    }
}
