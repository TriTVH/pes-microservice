using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs
{
    public record ProfileDto(
    int Id,
    string Email,
    string Name,
    string Phone,
    string Address,
    string AvatarUrl,
    string Gender,
    string Role
);
}
