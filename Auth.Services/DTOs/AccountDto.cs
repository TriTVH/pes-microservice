using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs
{
    public record AccountDto(int Id, string Email, string Name, string Role, string Status, string Phone, string Address, DateTime CreatedAt);
}
