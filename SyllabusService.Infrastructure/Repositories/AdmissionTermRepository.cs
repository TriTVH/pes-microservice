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

        public async Task<int> UpdateAdmissionTermAsync(AdmissionTerm admissionTerm)
        {
            _context.AdmissionTerms.Update(admissionTerm);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AdmissionTerm>> GetAdmissionTermsAsync()
        {
            return await _context.AdmissionTerms
                .Include(t => t.Classes)
                .ThenInclude(c => c.Syllabus)
                .ToListAsync();
        }

        public async Task<AdmissionTerm?> GetActiveAdmissionTerm()
        {
            return await _context.AdmissionTerms
                .Include(t => t.Classes)
                   .ThenInclude(c => c.Syllabus)
                .Include(t => t.Classes)
                   .ThenInclude(c => c.PatternActivities)
                .Where(t => t.Status.Equals("active")).FirstOrDefaultAsync();
        }

        public async Task<AdmissionTerm?> GetOverlappingTermAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AdmissionTerms
                .Where(term => startDate < term.EndDate && endDate > term.StartDate)
                .FirstOrDefaultAsync();
        }

        public async Task<AdmissionTerm?> GetOverlappingTermAsyncExceptId(DateTime startDate, DateTime endDate, int excludeId)
        {
            return await _context.AdmissionTerms
                .Where(term => startDate < term.EndDate && endDate > term.StartDate && term.Id != excludeId)
                .FirstOrDefaultAsync();
        }

        public async Task<AdmissionTerm?> GetAdmissionTermByIdAsync(int id)
        {
            return await _context.AdmissionTerms
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<AdmissionTerm>> GetPrioritizedAdmissionTermsAsync()
        {
            return await _context.AdmissionTerms
           .Where(t => !t.Status.Equals("inactive")) // Bỏ các term bị inactive
           .OrderByDescending(t => t.Status.Equals("active")) // Ưu tiên active trước
           .ThenByDescending(t => t.StartDate) // Sau đó sắp theo thời gian bắt đầu
           .ToListAsync();
        }

        public async Task UpdateStatusAuto()
        {
            var now = DateTime.UtcNow.AddHours(7); // Giờ VN (GMT+7)
            var terms = await _context.AdmissionTerms.ToListAsync();

            foreach (var term in terms)
            {
                string newStatus;

                if (now < term.StartDate)
                    newStatus = "inactive";
                else if (now >= term.StartDate && now <= term.EndDate)
                    newStatus = "active";
                else
                    newStatus = "blocked";

                if (term.Status != newStatus)
                {
                    term.Status = newStatus;
                    _context.Entry(term).State = EntityState.Modified;
                }
            }
            await _context.SaveChangesAsync();
        }

    }
}
