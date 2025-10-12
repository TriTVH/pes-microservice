using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories.IRepositories
{
    public interface IAdmissionTermRepo
    {
        Task<int> CreateAdmissionTermAsync(AdmissionTerm admissionTerm);
        Task<int> UpdateAdmissionTermAsync(AdmissionTerm admissionTerm);
        Task UpdateStatusAuto();
        Task<AdmissionTerm?> GetOverlappingTermAsync(DateTime startDate, DateTime endDate);
        Task<AdmissionTerm?> GetOverlappingTermAsyncExceptId(DateTime startDate, DateTime endDate, int excludeId);
        Task<IEnumerable<AdmissionTerm>> GetAdmissionTermsAsync();
        Task<AdmissionTerm?> GetAdmissionTermByIdAsync(int id);
        Task<AdmissionTerm?> GetActiveAdmissionTerm();
        Task<IEnumerable<AdmissionTerm>> GetPrioritizedAdmissionTermsAsync();
    }
}
