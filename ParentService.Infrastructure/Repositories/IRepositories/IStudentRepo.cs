using ParentService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Infrastructure.Repositories.IRepositories
{
    public interface IStudentRepo
    {
        Task<int> CreateStudentAsync(Student student);
        Task<bool> ExistByStudentNameAndParentId(string studentName, int parentId);
        Task<IEnumerable<Student>> GetStudentsAsyncByParentAccId(int parentAccId);
        Task<Student> GetStudentAsyncById(int studentId);
        Task<int> UpdateStudentAsync(Student student);
        Task<int> AddStudentClassAsync(StudentClass sc);
        Task<List<int>> GetClassIdsByStudentIdAsync(int studentId);
        Task<bool> CheckDuplicateNameStudentOfParent(int parentAccId, string studentName, int studentId);

    }
}
