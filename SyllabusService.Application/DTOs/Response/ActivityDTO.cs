using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Response
{
    public class ActivityDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly? Date { get; set; }
        public string DayOfWeek { get; set; }
        public TimeOnly? EndTime { get; set; }
        public TimeOnly? StartTime { get; set; }
    }
}
