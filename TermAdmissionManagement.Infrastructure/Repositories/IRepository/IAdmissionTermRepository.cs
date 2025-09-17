using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Infrastructure.Entities;

namespace TermAdmissionManagement.Infrastructure.Repositories.IRepository
{
    public interface IAdmissionTermRepository
    {
        Task<int> CreateAdmissionTermAsync(AdmissionTerm admissionTerm);
        Task<int> UpdateAdmissionTerm(AdmissionTerm admissionTerm);
        Task<AdmissionTerm?> GetByIdWithItemsAsync(int id);
        Task<AdmissionTerm?> GetByYear(int year);
        Task<IEnumerable<AdmissionTerm>?> GetAdmissionTermsAsync();
    }
}
