using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Request
{
    public class CheckClassRequest
    {
        public int StudentId { get; set; }

        public int CurrentClassId { get; set; } 
        public List<int> CheckedClassIds { get; set; }

    }
}
