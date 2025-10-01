using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Request
{
    public record CreateSyllabusRequest(string name, string description, int cost, int hoursOfSyllabus, string isActive);
}
