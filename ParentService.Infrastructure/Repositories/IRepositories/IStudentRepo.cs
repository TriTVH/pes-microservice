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
    }
}
