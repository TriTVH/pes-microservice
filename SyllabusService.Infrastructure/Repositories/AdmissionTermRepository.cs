using Microsoft.EntityFrameworkCore;
using SyllabusService.Infrastructure.DBContext;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories
{
    public class AdmissionTermRepository : IAdmissionTermRepo
    {

        private PES_APP_FULL_DBContext _context;
        public AdmissionTermRepository(PES_APP_FULL_DBContext context) 
        {
            _context = context;
        }
        public async Task<int> CreateAdmissionTermAsync(AdmissionTerm admissionTerm)
        {
             _context.AdmissionTerms.Add(admissionTerm);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AdmissionTerm>> GetAdmissionTermsAsync()
        {
            return await _context.AdmissionTerms
                .Include(t => t.Classes)
                .ToListAsync();
        }

        public async Task<AdmissionTerm?> GetOverlappingTermAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AdmissionTerms
                .Where(term => startDate < term.EndDate && endDate > term.StartDate)
                .FirstOrDefaultAsync();
        }

    }
}
