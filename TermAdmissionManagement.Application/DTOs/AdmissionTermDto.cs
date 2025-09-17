using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Application.DTOs
{
    public class AdmissionTermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Year { get; set; }
        public List<TermItemDTO> TermItems { get; set; } = new();
    }
}
