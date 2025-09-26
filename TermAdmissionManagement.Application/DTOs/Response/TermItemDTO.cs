using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Application.DTOs.Response
{
    public class TermItemDTO
    {
        public int Id { get; set; }
        public int? MaxNumberRegistration { get; set; }

        public int? CurrentRegisteredStudents { get; set; }

        public int? ExpectedClasses { get; set; }

        public string Status { get; set; }

        public int? DefaultFee { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Version { get; set; }

    }
}
