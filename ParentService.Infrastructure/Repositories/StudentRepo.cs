using Azure.Core;
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
        public async Task<Student?> GetStudentWithAdmissionFormsAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.AdmissionForms)
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task DeleteStudentAsync(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> ExistByStudentNameAndParentId(string studentName, int parentId)
        {
            return await _context.Students.AnyAsync(x => x.Name == studentName && x.ParentAccId == parentId);
        }

        public async Task<IEnumerable<Student>> GetStudentsAsyncByParentAccId(int parentAccId)
        {
            return await _context.Students.Where(x => x.ParentAccId == parentAccId).ToListAsync();
        }
        public async Task<Student> GetStudentAsyncById(int studentId)
        {
            return await _context.Students.Where(x => x.Id == studentId).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateStudentAsync(Student student)
        {
            _context.Students.Update(student);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> AddStudentClassAsync(StudentClass sc)
        {
            _context.StudentClasses.Add(sc);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetClassIdsByStudentIdAsync(int studentId)
        {
            return await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.ClassesId)
                .ToListAsync();
        }

        public async Task<bool> CheckDuplicateNameStudentOfParentExceptStudentId(int parentAccId, string studentName, int studentId)
        {
            return await _context.Students.AnyAsync(s =>
            s.ParentAccId == parentAccId &&
            s.Name.ToLower() == studentName.ToLower() &&
            s.Id != studentId // loại trừ chính bản thân
        );
        }

    }
}
