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
        Task<int> UpdateClassAsync(Class classes);
        Task<IEnumerable<Class>> GetClassesAsync();
        Task<List<Class>> GetClassesAfterDateInYearAsync(DateOnly endDate, int academicYear);

        Task<Class?> GetClassByIdAsync(int id);
        Task<List<Class>> GetClassesByIdsAsync(List<int> ids);
        Task<IEnumerable<Class>> GetActiveClassesByStudentId(int studentId);

        Task UpdateClassStatusAuto();

    }
}
