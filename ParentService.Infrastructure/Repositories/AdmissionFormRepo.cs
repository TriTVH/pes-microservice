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

        public async Task<AdmissionFormClass?> FindByAdmissionTermIdAndClassIdAndStudentNameAsync( int admissionTermId, int classId, string studentName )
        {
            return await _context.AdmissionFormClasses
               .Include(x => x.AdmissionForm)
               .ThenInclude(f => f.Student)
               .FirstOrDefaultAsync(x => x.AdmissionForm.AdmissionTermId == admissionTermId && x.ClassId == classId && x.AdmissionForm.Student.Name == studentName);
        }
    }
}
