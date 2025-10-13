using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.DTOs.Request
{
    public class WeekRequest
    {
        public DateOnly startWeek { get; set; }
        public DateOnly endWeek { get; set; }
    }
}
