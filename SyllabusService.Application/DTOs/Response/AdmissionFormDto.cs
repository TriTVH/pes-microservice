using SyllabusService.Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Response
{
    public class AdmissionFormDto
    {
        public int Id { get; set; }
        public AccountDto ParentAccount { get; set; }
        public StudentDTO Student { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string CancelReason { get; set; }
        public string Status { get; set; }

    }
}
