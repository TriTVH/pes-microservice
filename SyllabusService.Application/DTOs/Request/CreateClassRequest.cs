using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Request
{
    public record CreateClassRequest( DateOnly startDate, int syllabusId, int teacherId, List<ActivityRequest> activities);
    public record ActivityRequest(string dayOfWeek,
 TimeOnly startTime,
 TimeOnly endTime );
}
