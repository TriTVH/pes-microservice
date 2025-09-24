using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Infrastructure.Entities;

namespace TermAdmissionManagement.Infrastructure.Repositories.IRepository
{
    public interface IAdmissionFormRepository
    {
        Task<IEnumerable<AdmissionForm>> GetAllAsync();
        Task<int> CreateAdmissionForm(AdmissionForm admissionForm);
        Task<AdmissionForm> findByTermItemIdAndStatusAndChild(int termId, string status, int childId);
    
        Task<IEnumerable<AdmissionForm>> findByParentAccountId(int accountId);
    }
}
