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
    public class WeekRepositoryTest
    {
        private readonly PES_APP_FULL_DBContext _context;
        private readonly WeekRepository _repository;

        public WeekRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<PES_APP_FULL_DBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // mỗi test DB riêng biệt
                .Options;

            _context = new PES_APP_FULL_DBContext(options);
            _context.Database.EnsureCreated();

            _repository = new WeekRepository(_context);
        }

        [Fact]
        public async Task GetSchedulesByClassIdAsync_ShouldReturnSchedulesForClass()
        {
            // Arrange
            var class1Schedules = new List<Schedule>
            {
                new Schedule { Id = 1, ClassesId = 1 },
                new Schedule { Id = 2, ClassesId = 1 }
            };

            var class2Schedules = new List<Schedule>
            {
                new Schedule { Id = 3, ClassesId = 2 }
            };

            await _context.Schedules.AddRangeAsync(class1Schedules.Concat(class2Schedules));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSchedulesByClassIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.All(x => x.ClassesId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task GetSchedulesByClassIdAsync_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            await _context.Schedules.AddRangeAsync(
                new Schedule { Id = 10, ClassesId = 99 },
                new Schedule { Id = 11, ClassesId = 98 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSchedulesByClassIdAsync(1);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
