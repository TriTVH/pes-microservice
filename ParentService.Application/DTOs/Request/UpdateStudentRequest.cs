using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.DTOs.Request
{
    public class UpdateStudentRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PlaceOfBirth { get; set; }
        public string ProfileImage { get; set; }
        public string BirthCertificateImg { get; set; }
        public string HouseholdRegistrationImg { get; set; }
    }
}
