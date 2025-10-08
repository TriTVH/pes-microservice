using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.DTOs.Response
{
    public class StudentDTO
    {
        public int Id { get; set; }

        public string BirthCertificateImg { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string HouseholdRegistrationImg { get; set; }

        public string Gender { get; set; }

        public string Name { get; set; }

        public string PlaceOfBirth { get; set; }

        public string ProfileImage { get; set; }

        public bool? IsStudent { get; set; }


    }
}
