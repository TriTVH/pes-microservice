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
    public class AdmissionTermRepository : IAdmissionTermRepository
    {
        private readonly PesTermManagementContext _context;

        public AdmissionTermRepository(PesTermManagementContext context)
        {
            _context = context;
        }
        public async Task<int> CreateAdmissionTermAsync(AdmissionTerm admissionTerm)
        {
            _context.Add(admissionTerm);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateAdmissionTerm(AdmissionTerm admissionTerm)
        {
            _context.AdmissionTerms.Update(admissionTerm);
            return await _context.SaveChangesAsync();
        }
        public async Task<AdmissionTerm?> GetByIdWithItemsAsync(int id)
        {
            return await _context.AdmissionTerms
                .Include(t => t.TermItems)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<AdmissionTerm>?> GetAdmissionTermsAsync()
        {
            return await _context.AdmissionTerms
     .OrderByDescending(x => x.Year)
     .Include("TermItems")
     .ToListAsync();
        }

        public async Task<AdmissionTerm?> GetByYear(int year)
        {
            return await _context.AdmissionTerms.FirstOrDefaultAsync(x => x.Year == year);
        }


    }
}
