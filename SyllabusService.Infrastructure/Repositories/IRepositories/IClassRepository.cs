using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories.IRepositories
{
    public interface IClassRepository
    {
        Task<int> CreateClassAsync(Class classes);
        Task<Class?> GetClassByYearAndSyllabusId(int year, int syllabusId);

        Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId);
        Task<List<Class>> GetExistingClassIdsAsync(List<int> classIds);
        Task<int> UpdateClassAsync(Class classes);
        Task<IEnumerable<Class>> GetClassesAsync();
        Task<List<Class>> GetClassesAfterDateInYearAsync(DateOnly endDate, int academicYear);
    }
}
