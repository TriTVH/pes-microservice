using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories.IRepositories
{
    public interface ISyllabusRepository
    {
        Task<int> CreateSyllabusAsync(Syllabus syllabus);
        Task<int> UpdateSyllabusAsync(Syllabus syllabus);
        Task<Syllabus> GetSyllabusByIdAsync(int id);
        Task<Syllabus?> GetSyllabusByNameAsync(string name);
        Task<IEnumerable<Syllabus>> GetAllSyllabusAsync();
        Task<bool> IsDuplicateNameAsync(string name, int id);
    }
}
