using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs
{
    public record ForgotPasswordSimpleResponseDto(
        string ResetToken,
        string Message,
        DateTime ExpiresAt
    );
}
