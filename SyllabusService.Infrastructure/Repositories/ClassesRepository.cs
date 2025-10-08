﻿using Microsoft.EntityFrameworkCore;
using SyllabusService.Infrastructure.DBContext;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories
{
    public class ClassesRepository : IClassRepository
    {
        private PES_APP_FULL_DBContext _context;

        public ClassesRepository(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }
        public async Task<int> CreateClassAsync(Class classes)
        {
            _context.Classes.Add(classes);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateClassAsync(Class classes)
        {
            _context.Classes.Update(classes);
            return await _context.SaveChangesAsync();
        }
        public async Task<Class?> GetClassByYearAndSyllabusId(int year, int syllabusId)
        {
            return await _context.Classes
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.AcademicYear == year && x.SyllabusId == syllabusId);
        }



        public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId)
        {
            return await _context.Classes
                 .Include(x => x.Schedules)
                   .ThenInclude(s => s.Activities)
                         .Where(x => x.TeacherId == teacherId).ToListAsync();
        }


        public async Task<IEnumerable<Class>> GetClassesAsync()
        {
            return await _context.Classes.ToListAsync();
        }

    
        public async Task<IEnumerable<Class>> GetClassesWithPatternActiviAsync(List<int> classIds)
        {
            if (classIds == null || !classIds.Any())
                return new List<Class>();

            return await _context.Classes
                .Where(c => classIds.Contains(c.Id))
                .Include(c => c.PatternActivities)
                .ToListAsync();
        }

        public async Task<List<Class>> GetExistingClassIdsAsync(List<int> classIds)
        {
            return await _context.Classes
                .Where(c => classIds.Contains(c.Id))
                .ToListAsync();
        }
        public async Task<List<Class>> GetClassesAfterDateInYearAsync(DateOnly endDate, int academicYear)
        {
            return await _context.Classes
                .Where(c =>
                    c.StartDate > endDate &&
                    c.AcademicYear == academicYear &&
                    c.NumberStudent < 30)
                .OrderByDescending (x => x.StartDate)
                .ToListAsync();
        }

        public async Task<List<Class>> GetClassesByIdsAsync(List<int> ids)
        {
            return await _context.Classes
                .Include(c => c.PatternActivities)
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<Class?> GetClassByIdAsync(int id)
        {
            return await _context.Classes
                .Include(c => c.PatternActivities)
                .FirstOrDefaultAsync(c => c.Id == id);
        }


    }
}
