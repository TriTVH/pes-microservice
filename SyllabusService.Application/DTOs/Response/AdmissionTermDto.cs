using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Response
{
    public record AdmissionTermDto(
        int Id,
        int? AcademicYear,
        int? MaxNumberRegistration,
        int? CurrentRegisteredStudents,
        int? NumberOfClasses,
        DateTime StartDate,
        DateTime EndDate,
        string Status,
        List<ClassDto> ClassDtos);
}

