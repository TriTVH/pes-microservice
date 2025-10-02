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

        public int? NumberStudent { get; set; }
        public int? AcademicYear { get; set; }
        public int? NumberOfWeeks { get; set; }

        public string Status { get; set; }
    }
}
