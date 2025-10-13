using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Domain.DTOs.Request
{
    public class GetActivitiesBetweenStartDateAndEndDateRequest
    {
        public List<int?> classIds {  get; set; }
        public DateOnly startWeek {  get; set; }
        public DateOnly endWeek { get; set; }
    }
}
