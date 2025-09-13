using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs
{
    public record RegisterRequestDto(string Email, string Password, string Name, string Role);
}
