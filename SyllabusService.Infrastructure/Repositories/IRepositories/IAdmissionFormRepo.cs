using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories.IRepositories
{
    public interface IAdmissionFormRepo
    {
        Task<IEnumerable<AdmissionForm>> GetAdmissionFormsByAdmissionTermIdAsync(int admissionTermId);
        Task<int> UpdateAdmissionFormAsync(AdmissionForm admissionForm);
        Task<AdmissionForm?> GetAdmissionFormByIdAsync(int id);
        Task UpdateAdmissionFormsOverDueDateAuto();
    }
}
