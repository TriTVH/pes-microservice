using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }   // reset token đã gửi qua email
        public string NewPassword { get; set; }
    }
}
