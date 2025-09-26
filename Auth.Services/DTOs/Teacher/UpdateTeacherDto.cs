using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs.Teacher
{
    public record UpdateTeacherDto(
    string? Name,
    string? Phone,
    string? Address,
    string? AvatarUrl,
    string? Gender,
    string? IdentityNumber
);
}
