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
    public class AdmissionFormRepo : IAdmissionFormRepo
    {
        private PES_APP_FULL_DBContext _context;

        public AdmissionFormRepo(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }

        public async Task<int> UpdateAdmissionFormAsync(AdmissionForm admissionForm)
        {
            _context.AdmissionForms.Update(admissionForm);
            return await _context.SaveChangesAsync();
        }

        public async Task<AdmissionForm?> GetAdmissionFormByIdAsync(int id)
        {
            return await _context.AdmissionForms.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<AdmissionForm>> GetAdmissionFormsByAdmissionTermIdAsync(int admissionTermId)
        {
            return await _context.AdmissionForms
                .Where(x => x.AdmissionTermId == admissionTermId)
         .OrderByDescending(af => af.Status == "waiting_for_approve") // Ưu tiên waiting_for_approve lên đầu
         .ThenByDescending(af => af.SubmittedDate) // Sau đó sắp theo SubmittedDate
         .ToListAsync();
        }

        public async Task UpdateAdmissionFormsOverDueDateAuto()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var overdueForms = await _context.AdmissionForms
           .Include(f => f.AdmissionTerm)
           .Where(f => f.AdmissionTerm.EndDate < now && (f.Status == "waiting_for_payment" || f.Status == "waiting_for_approve"))
           .ToListAsync();
            if (overdueForms.Any())
            {
                foreach (var form in overdueForms)
                {
                    form.Status = "over_due_date";
                }

                await _context.SaveChangesAsync();
            }
        }

    }
}
