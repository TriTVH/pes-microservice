using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Request
{
    public class UpdateAdmissionFormStatusRequest
    {
        public int Id { get; set; }
        public string Action { get; set; }

        public string? CancelReason { get; set; }
    }
}
