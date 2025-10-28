using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SyllabusService.Infrastructure.DBContext;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit.Tests.Repositories
{
    public class ClassesRepositoryTest
    {
        private readonly PES_APP_FULL_DBContext _context;
        private readonly ClassesRepository _repository;

        public ClassesRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<PES_APP_FULL_DBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging() // 👈 Giúp debug lỗi entity
                .Options;

            _context = new PES_APP_FULL_DBContext(options);
            _context.Database.EnsureCreated();
            _repository = new ClassesRepository(_context);
        }

        [Fact]
        public async Task CreateClassAsync_ShouldAddSuccessfully()
        {
            var cls = new Class
            {
                Id = 1,
                Name = "SE1815",
                AcademicYear = 2025,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                Status = "inactive",
                TeacherId = 1
            };

            var result = await _repository.CreateClassAsync(cls);
            result.Should().Be(1);
            (await _context.Classes.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task UpdateClassAsync_ShouldModifySuccessfully()
        {
            var cls = new Class { Id = 2, Name = "PRN222", Status = "inactive", TeacherId = 1 };
            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();

            cls.Status = "active";
            var result = await _repository.UpdateClassAsync(cls);
            result.Should().Be(1);
            (await _context.Classes.FindAsync(2))!.Status.Should().Be("active");
        }

        [Fact]
        public async Task GetClassByYearAndSyllabusId_ShouldReturnCorrectClass()
        {
            var syllabus = new Syllabus { Id = 1, Name = "DBI202" };
            _context.Syllabi.Add(syllabus);
            _context.Classes.AddRange(
                new Class { Id = 3, AcademicYear = 2024, SyllabusId = 1, CreatedAt = DateTime.UtcNow.AddDays(-1), TeacherId = 1 },
                new Class { Id = 4, AcademicYear = 2025, SyllabusId = 1, CreatedAt = DateTime.UtcNow.AddHours(1), TeacherId = 1 }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetClassByYearAndSyllabusId(2025, 1);
            result.Should().NotBeNull();
            result!.Id.Should().Be(4);
        }

        [Fact]
        public async Task GetExistingClassesByTeacherIdAsync_ShouldReturnOnlyActiveOrInactive()
        {
            var clsList = new List<Class>
            {
                new Class { Id = 5, TeacherId = 1, Status = "active", Schedules = new List<Schedule> { new Schedule() } },
                new Class { Id = 6, TeacherId = 1, Status = "inactive", Schedules = new List<Schedule> { new Schedule() } },
                new Class { Id = 7, TeacherId = 1, Status = "done" }
            };
            _context.Classes.AddRange(clsList);
            await _context.SaveChangesAsync();

            var result = await _repository.GetExistingClassesByTeacherIdAsync(1);
            result.Should().HaveCount(2);
            result.All(c => c.Status is "active" or "inactive").Should().BeTrue();
        }

        [Fact]
        public async Task GetClassesAsync_ShouldIncludeSyllabus()
        {
            _context.Classes.Add(new Class
            {
                Id = 8,
                Name = "C#",
                Syllabus = new Syllabus { Id = 2, Name = "PRN" },
                TeacherId = 1
            });
            await _context.SaveChangesAsync();

            var result = await _repository.GetClassesAsync();
            result.Should().HaveCount(1);
            result.First().Syllabus.Should().NotBeNull();
        }

        [Fact]
        public async Task GetClassesWithPatternActiviAsync_ShouldReturnEmpty_WhenIdsNullOrEmpty()
        {
            var result1 = await _repository.GetClassesWithPatternActiviAsync(null);
            var result2 = await _repository.GetClassesWithPatternActiviAsync(new List<int>());
            result1.Should().BeEmpty();
            result2.Should().BeEmpty();
        }

        [Fact]
        public async Task GetClassesWithPatternActiviAsync_ShouldReturnMatchingClasses()
        {
            _context.Classes.AddRange(
                new Class
                {
                    Id = 9,
                    PatternActivities = new List<PatternActivity>
                    {
                        new PatternActivity
                        {
                            Id = 1,
                            DayOfWeek = "MONDAY",
                            StartTime = new TimeOnly(8, 0),
                            EndTime = new TimeOnly(9, 0)
                        }
                    },
                    Syllabus = new Syllabus { Id = 3, Name = "SWP" },
                    TeacherId = 1
                },
                new Class { Id = 10, Syllabus = new Syllabus { Id = 4, Name = "PRM" }, TeacherId = 1 }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetClassesWithPatternActiviAsync(new List<int> { 9 });
            result.Should().ContainSingle(c => c.Id == 9);
        }

        [Fact]
        public async Task GetClassesAfterDateInYearAsync_ShouldReturnCorrectFilteredClasses()
        {
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            _context.Classes.AddRange(
                new Class { Id = 11, StartDate = endDate.AddDays(1), AcademicYear = endDate.Year, NumberStudent = 20, Syllabus = new Syllabus { Id = 1 }, TeacherId = 1 },
                new Class { Id = 12, StartDate = endDate.AddDays(-1), AcademicYear = endDate.Year, NumberStudent = 25, Syllabus = new Syllabus { Id = 2 }, TeacherId = 1 },
                new Class { Id = 13, StartDate = endDate.AddDays(5), AcademicYear = endDate.Year + 1, NumberStudent = 10, Syllabus = new Syllabus { Id = 3 }, TeacherId = 1 }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetClassesAfterDateInYearAsync(endDate, endDate.Year);
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(11);
        }

        [Fact]
        public async Task GetClassesByIdsAsync_ShouldReturnClassesWithIncludes()
        {
            _context.Classes.Add(new Class
            {
                Id = 14,
                Syllabus = new Syllabus { Id = 5, Name = "PRN" },
                PatternActivities = new List<PatternActivity>
                {
                    new PatternActivity
                    {
                        Id = 1,
                        DayOfWeek = "WEDNESDAY",
                        StartTime = new TimeOnly(10, 0),
                        EndTime = new TimeOnly(11, 0)
                    }
                },
                TeacherId = 1
            });
            await _context.SaveChangesAsync();

            var result = await _repository.GetClassesByIdsAsync(new List<int> { 14 });
            result.Should().HaveCount(1);
            result.First().Syllabus.Should().NotBeNull();
            result.First().PatternActivities.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetClassByIdAsync_ShouldReturnClassWithIncludes()
        {
            _context.Classes.Add(new Class
            {
                Id = 15,
                Syllabus = new Syllabus { Id = 6, Name = "OOP" },
                PatternActivities = new List<PatternActivity>
                {
                    new PatternActivity
                    {
                        Id = 2,
                        DayOfWeek = "FRIDAY",
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(10, 0)
                    }
                },
                TeacherId = 1
            });
            await _context.SaveChangesAsync();

            var result = await _repository.GetClassByIdAsync(15);
            result.Should().NotBeNull();
            result!.Syllabus.Should().NotBeNull();
            result.PatternActivities.Should().NotBeEmpty();
        }

      

    }

}
