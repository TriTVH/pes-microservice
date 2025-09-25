using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Account account);
        string GenerateTokenForLogin(Account account);
        string GeneratePasswordResetToken(Account account); 
        ClaimsPrincipal ValidatePasswordResetToken(string token);
    }
}
