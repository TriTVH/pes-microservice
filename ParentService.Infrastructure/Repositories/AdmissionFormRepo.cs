using Microsoft.EntityFrameworkCore;
using ParentService.Infrastructure.Models;
using ParentService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Infrastructure.Repositories
{
    public class AdmissionFormRepo : IAdmissionFormRepo
    {
        private PES_APP_FULL_DBContext _context;

        public AdmissionFormRepo( PES_APP_FULL_DBContext context )
        {
            _context = context;
        }
        public async Task<int> CreateAdmissionFormAsync( AdmissionForm admissionForm )
        {
            _context.AdmissionForms.Add(admissionForm);
            return await _context.SaveChangesAsync();
        }

        public async Task CreateAdmissionFormClassesAsync(List<AdmissionFormClass> formClasses)
        {
            _context.AdmissionFormClasses.AddRange(formClasses);
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAdmissionForm(AdmissionForm admissionForm)
        {
            _context.AdmissionForms.Remove(admissionForm);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AdmissionForm>> GetAdmissionFormsByParentAccountIdAsync(int parentAccountId)
        {
            return await _context.AdmissionForms.Where(x => x.ParentAccountId == parentAccountId).OrderByDescending(x => x.SubmittedDate).ToListAsync();
        }

        public async Task<AdmissionFormClass?> FindByAdmissionTermIdAndClassIdAndStudentNameAsync( int admissionTermId, int classId, string studentName )
        {
            return await _context.AdmissionFormClasses
               .Include(x => x.AdmissionForm)
               .ThenInclude(f => f.Student)
               .FirstOrDefaultAsync(x => x.AdmissionForm.AdmissionTermId == admissionTermId && x.ClassId == classId && x.AdmissionForm.Student.Name == studentName);
        }

        public async Task<string?> GetStudentAdmissionFormStatusAsync(int studentId, int classId)
        {
            var admissionFormClass = await _context.AdmissionFormClasses
                .Include(afc => afc.AdmissionForm)
                .Where(afc => afc.ClassId == classId && afc.AdmissionForm.StudentId == studentId)
                .OrderByDescending(afc => afc.AdmissionForm.SubmittedDate) 
                .Select(afc => afc.AdmissionForm.Status)
                .FirstOrDefaultAsync();

            return admissionFormClass; 
        }
        
        public async Task<List<int>> GetClassIdsByAdmissionFormId(int afId)
        {
            return await _context.AdmissionFormClasses
                .Where(x => x.AdmissionFormId == afId)
                .Select(x => x.ClassId)
                .ToListAsync();
        }
        public async Task<AdmissionForm?> GetAdmissionFormByIdAsync(int id)
        {
            return await _context.AdmissionForms
                .Include(x => x.AdmissionFormClasses)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> UpdateAdmissionFormAsync(AdmissionForm admissionForm)
        {
            _context.AdmissionForms.Update(admissionForm);
            return await _context.SaveChangesAsync();
        }

    }
}
