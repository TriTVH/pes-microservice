using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs.Teacher
{
    public record CreateTeacherDto(
    string Email,
    string Name,
    string Password 
);
}
