using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Application.DTOs.Response
{
    public class AdmissionFormDto
    {
        public int Id { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string? CancelReason { get; set; }

        public string ChildCharacteristicsFormImg { get; set; }

        public string CommitmentImg { get; set; }

        public string HouseholdRegistrationAddress { get; set; }

        public string Note { get; set; }

        public DateTime? PaymentExpiryDate { get; set; }

        public string Status { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public string termItemGrade { get; set; }

        public string ParentName { get; set; }
        public string ParentEmail { get; set; }
        public string ParentPhoneNumber { get; set; }


    }
}
