using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs.Teacher
{
    public class TeacherScheduleDto
    {
        public string ClassName { get; set; }
        public string WeekName { get; set; }
        public List<ActivityDto> Activities { get; set; }
    }
    public class ActivityDto
    {
        public string Name { get; set; }
        public DateOnly? Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }
}
