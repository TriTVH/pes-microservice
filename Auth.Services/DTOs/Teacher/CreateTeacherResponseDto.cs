using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs.Teacher
{
    public record CreateTeacherResponseDto(
        int Id,
        string Email,
        string Name,
        string GeneratedPassword,
        string Message
    );
}
