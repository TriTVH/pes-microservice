using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Infrastructure.DBContext;
using TermAdmissionManagement.Infrastructure.Entities;
using TermAdmissionManagement.Infrastructure.Repositories.IRepository;

namespace TermAdmissionManagement.Infrastructure.Repositories
{
    public class AdmissionFormRepository : IAdmissionFormRepository
    {
        private readonly PesTermManagementContext _context;

        public AdmissionFormRepository(PesTermManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdmissionForm>> GetAllAsync()
        {
            return await _context.AdmissionForms
                .AsNoTracking()
                .Include(x => x.TermItem)
                .ToListAsync();
        }

        public async Task<int> CreateAdmissionForm(AdmissionForm admissionForm)
        {
            _context.Add(admissionForm);
            return await _context.SaveChangesAsync();
        }

        public async Task<AdmissionForm> findByTermItemIdAndStatusAndChild(int termId, string status, int childId)
        {
            return await _context.AdmissionForms
                .FirstOrDefaultAsync(x => x.TermItemId == termId && x.Status == status && x.StudentId ==  childId);
        }

        public async Task<IEnumerable<AdmissionForm>> findByParentAccountId(int accountId)
        {
            return _context.AdmissionForms
                .Where(x => x.ParentAccountId == accountId)
                .ToList();
        }
    }
}
