using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response
{
    public class AuthResponse
    {
        public TokenResponse token { get; set; }
        public UserResponse User { get; set; }
    }
}
