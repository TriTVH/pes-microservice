using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Response
{
    public class SyllabusDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Cost { get; set; }
        public int HoursOfSyllabus { get; set; }
        public string IsActive { get; set; }
    }
}
