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
        public string Grade { get; set; } = "";

        public int? MaxNumberRegistration { get; set; }
        public int? ExpectedClasses { get; set; }

        public int DefaultFee { get; set; }

        public string Status { get; set; } = "";
    }
}
