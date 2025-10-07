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
    public class StudentRepo : IStudentRepo
    {
        private PES_APP_FULL_DBContext _context;

        public StudentRepo(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }
        public async Task<int> CreateStudentAsync(Student student)
        {
            _context.Students.Add(student);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistByStudentNameAndParentId(string studentName, int parentId)
        {
            return await _context.Students.AnyAsync(x => x.Name == studentName && x.ParentAccId == parentId);
        }

        public async Task<IEnumerable<Student>> GetStudentsAsyncByParentAccId(int parentAccId)
        {
            return await _context.Students.Where(x => x.ParentAccId == parentAccId).ToListAsync();
        }

    }
}
