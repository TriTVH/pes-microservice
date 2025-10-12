using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.DTOs.Response
{
    public class AdmissionFormDto
    {
        public int Id { get; set; }
        public StudentDTO Student { get; set; }

        public DateTime AdmissionTermStartDate { get; set; }
        public DateTime AdmissionTermEndDate { get; set; }


        public DateTime? SubmittedDate { get; set; }
        public DateTime? PaymentExpiryDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string CancelReason { get; set; }
        public string Status { get; set; }

    }
}
