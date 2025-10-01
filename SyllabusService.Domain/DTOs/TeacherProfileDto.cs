using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Domain.DTOs
{
   public record TeacherProfileDto(
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
