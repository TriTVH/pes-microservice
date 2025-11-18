using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Response
{
    public class ClassDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string teacherName { get; set; }

        public string teacherEmail { get; set; }

        public int? NumberStudent { get; set; }
        public int? AcademicYear { get; set; }
        public int? NumberOfWeeks { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? Cost { get; set; }
        public string Status { get; set; }

        public List<PatternActivityDto> PatternActivitiesDTO { get; set; }
    }

    public class PatternActivityDto
    {
        public string DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }

}
