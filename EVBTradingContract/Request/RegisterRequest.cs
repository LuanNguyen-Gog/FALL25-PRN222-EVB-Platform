using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Request
{
    public class RegisterRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = default!;

        [Required, MinLength(4), StringLength(100)]
        public string Password { get; set; } = default!;
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string? Phone { get; set; } = default!;
    }
}
