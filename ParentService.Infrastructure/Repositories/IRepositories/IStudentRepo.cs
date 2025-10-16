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
        Task<int> UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(Student student);
        Task<IEnumerable<Student>> GetStudentsAsyncByParentAccId(int parentAccId);
        Task<Student> GetStudentAsyncById(int studentId);
        Task<int> AddStudentClassAsync(StudentClass sc);
        Task<List<int>> GetClassIdsByStudentIdAsync(int studentId);
        Task<Student?> GetStudentWithAdmissionFormsAsync(int studentId);
        Task<bool> CheckDuplicateNameStudentOfParentExceptStudentId(int parentAccId, string studentName, int studentId);
        Task<bool> ExistByStudentNameAndParentId(string studentName, int parentId);

    }
}
