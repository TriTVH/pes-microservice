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
    public class ActivityRepositoryTest
    {
        private readonly PES_APP_FULL_DBContext _context;
        private readonly ActivityRepository _repository;

        public ActivityRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<PES_APP_FULL_DBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging() // để debug nếu lỗi EF
                .Options;

            _context = new PES_APP_FULL_DBContext(options);
            _context.Database.EnsureCreated();
            _repository = new ActivityRepository(_context);
        }

        [Fact]
        public async Task GetActivitiesByScheduleIdAsync_ShouldReturnMatchingActivities()
        {
            // Arrange
            var schedule1 = new Schedule { Id = 1, ClassesId = 10 };
            var schedule2 = new Schedule { Id = 2, ClassesId = 11 };

            _context.Schedules.AddRange(schedule1, schedule2);
            await _context.SaveChangesAsync();

            var activities = new List<Activity>
            {
                new Activity { Id = 1, ScheduleId = 1, Name = "A", DayOfWeek = "MONDAY", StartTime = new TimeOnly(8,0), EndTime = new TimeOnly(9,0), Date = DateOnly.FromDateTime(DateTime.UtcNow) },
                new Activity { Id = 2, ScheduleId = 1, Name = "B", DayOfWeek = "TUESDAY", StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(10,0), Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) },
                new Activity { Id = 3, ScheduleId = 2, Name = "C", DayOfWeek = "WEDNESDAY", StartTime = new TimeOnly(10,0), EndTime = new TimeOnly(11,0), Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)) }
            };
            await _context.Activities.AddRangeAsync(activities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActivitiesByScheduleIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.All(a => a.ScheduleId == 1).Should().BeTrue();
        }

        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldReturnFilteredAndOrderedActivities()
        {
            // Arrange
            var weekStart = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekEnd = weekStart.AddDays(7);

            var schedules = new List<Schedule>
            {
                new Schedule { Id = 10, ClassesId = 100 },
                new Schedule { Id = 11, ClassesId = 101 }
            };
            await _context.Schedules.AddRangeAsync(schedules);
            await _context.SaveChangesAsync();

            var activities = new List<Activity>
            {
                new Activity
                {
                    Id = 4,
                    Name = "Early Activity",
                    ScheduleId = 10,
                    Schedule = schedules[0],
                    DayOfWeek = "MONDAY",
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(9, 0),
                    Date = weekStart.AddDays(1)
                },
                new Activity
                {
                    Id = 5,
                    Name = "Late Activity",
                    ScheduleId = 10,
                    Schedule = schedules[0],
                    DayOfWeek = "MONDAY",
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(11, 0),
                    Date = weekStart.AddDays(1)
                },
                new Activity
                {
                    Id = 6,
                    Name = "Out of range",
                    ScheduleId = 10,
                    Schedule = schedules[0],
                    DayOfWeek = "MONDAY",
                    StartTime = new TimeOnly(12, 0),
                    EndTime = new TimeOnly(13, 0),
                    Date = weekEnd.AddDays(2) // ngoài khoảng
                },
                new Activity
                {
                    Id = 7,
                    Name = "Different Class",
                    ScheduleId = 11,
                    Schedule = schedules[1],
                    DayOfWeek = "TUESDAY",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0),
                    Date = weekStart.AddDays(2)
                }
            };

            await _context.Activities.AddRangeAsync(activities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActivitiesBetweenStartDateAndEndDate(
                new List<int?> { 100, 101 }, weekStart, weekEnd);

            // Assert
            result.Should().HaveCount(3); // bỏ activity ngoài range
            result.Should().BeInAscendingOrder(a => a.Date);

            // Đảm bảo sorting đúng: Early trước Late
            result.First().Name.Should().Be("Early Activity");
        }

        [Fact]
        public async Task GetActivitiesBetweenStartDateAndEndDate_ShouldReturnEmpty_WhenNoMatch()
        {
            var weekStart = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekEnd = weekStart.AddDays(7);

            var schedule = new Schedule { Id = 20, ClassesId = 200 };
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            var activity = new Activity
            {
                Id = 8,
                ScheduleId = 20,
                Schedule = schedule,
                DayOfWeek = "MONDAY",
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(9, 0),
                Date = weekStart.AddDays(-5) // trước tuần
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            var result = await _repository.GetActivitiesBetweenStartDateAndEndDate(
                new List<int?> { 300 }, weekStart, weekEnd);

            result.Should().BeEmpty();
        }
    }
}
