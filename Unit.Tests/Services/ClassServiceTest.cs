using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services;
using SyllabusService.Domain.DTOs.Response;
using SyllabusService.Domain.IClient;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Tests.Services

{
    public class ClassServiceTest
    {
        private readonly Mock<IClassRepository> _mockClassRepo;
        private readonly Mock<ISyllabusRepository> _mockSyllabusRepo;
        private readonly Mock<IAuthClient> _mockAuthClient;
        private readonly ClassesService _service;

        public ClassServiceTest()
        {
            _mockClassRepo = new Mock<IClassRepository>();
            _mockSyllabusRepo = new Mock<ISyllabusRepository>();
            _mockAuthClient = new Mock<IAuthClient>();

            _service = new ClassesService(
                _mockSyllabusRepo.Object,
                _mockClassRepo.Object,
                _mockAuthClient.Object
            );
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_StartDateInvalid()
        {
           
            var req = new CreateClassRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), 1, 2, new List<ActivityRequest>
            {
                new("Monday", new TimeOnly(8,0), new TimeOnly(09,0))
            });
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Start date cannot be in the past.");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidNumberActivitiesSmallThanZero()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10 - (int)DateTime.UtcNow.DayOfWeek));
            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>());
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Please add at least one activity");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidRange()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10 - (int)DateTime.UtcNow.DayOfWeek));
            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>()
            {
        new("Monday", new TimeOnly(8, 0), new TimeOnly(9, 0)),
        new("Tuesday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Wednesday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Thursday", new TimeOnly(13, 0), new TimeOnly(14, 0)),
        new("Friday", new TimeOnly(14, 0), new TimeOnly(15, 0)) 
    }
);
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("You can add up to 4 activities");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidNameDayOfWeek()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10 - (int)DateTime.UtcNow.DayOfWeek));
            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>()
            {
        new("Monday", new TimeOnly(8, 0), new TimeOnly(9, 0)),
        new("Tuesday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Wednesday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Thursday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
  );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Start date must match the dayOfWeek of the first activity");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidSendingListDayOfWeekBySorting()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));

            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(8, 0), new TimeOnly(9, 0)),
        new("Tuesday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Wednesday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Thursday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
);
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("must be after previous one");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidSendingListDayOfWeek_DuplicateActivityCombo()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));


            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Sunday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Duplicate activity combo");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidSendingListDayOfWeek_ActivityTimeMustbeFullHour()
        {

            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));


            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 12), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("StartTime and EndTime must be in full hour format like");
        }
        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidSendingListDayOfWeek_InvalidActivityTime()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));


            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(10, 0), new TimeOnly(9, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("EndTime must be after StartTime.");
        }
        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidSendingListDayOfWeek_InvalidActivityTimeMustbeExactlyOneHour()
        {

            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));


            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(12, 0)),
        new("Sunday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Each activity must last exactly 1 hour.");
        }
        [Fact]
        public async Task CreateClass_ShouldReturnBadRequest_InvalidSendingListDayOfWeek_MaxNumberOfActivitiesPerDay()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));


            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Friday", new TimeOnly(11, 0), new TimeOnly(12, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Each dayOfWeek can have at most 2 hours of activities");
        }



        [Fact]
        public async Task CreateClass_ShouldReturnNotFound_SyllabusNotExist()
        {

            _mockSyllabusRepo.Setup(x => x.GetSyllabusByIdAsync(It.IsAny<int>()))
                 .ReturnsAsync((Syllabus?)null);
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));

            var req = new CreateClassRequest(friday, 2, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0)),
        new("Sunday", new TimeOnly(14, 0), new TimeOnly(15, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("notFound");
            result.Message.Should().Contain("Syllabus not found or be deleted");
        }

        [Fact]
        public async Task CreateClass_ShouldReturnNotFound_TeacherNotFound()
        {

            _mockSyllabusRepo.Setup(x => x.GetSyllabusByIdAsync(It.IsAny<int>()))
                  .ReturnsAsync(new Syllabus
                  {
                      Id = 1,
                      Name = "PRN222",
                      HoursOfSyllabus = 20,
                      IsActive = true
                  });

            _mockAuthClient.Setup(x => x.GetTeacherProfileDtoById(It.IsAny<int>()))
                  .ReturnsAsync((AccountDto?)null);
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));



            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0)),
        new("Sunday", new TimeOnly(14, 0), new TimeOnly(15, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("notFound");
            result.Message.Should().Contain("Teacher not found or be deleted");
        }




        [Fact]
        public async Task CreateClass_ShouldReturnConflict_WhenTeacherHasOverlappingClass()
        {

            _mockSyllabusRepo.Setup(x => x.GetSyllabusByIdAsync(It.IsAny<int>()))
                  .ReturnsAsync(new Syllabus
                  {
                      Id = 1,
                      Name = "PRN222",
                      HoursOfSyllabus = 20,
                      IsActive = true
                  });

            _mockAuthClient.Setup(x => x.GetTeacherProfileDtoById(It.IsAny<int>()))
    .ReturnsAsync(new AccountDto(2, "huatri2004@gmail.com", "teacher12", "TEACHER","ACCOUNT_ACTIVE","0918001135", "47 PNT", DateTime.UtcNow));
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));



            _mockClassRepo.Setup(x => x.GetExistingClassesByTeacherIdAsync(It.IsAny<int>()))
      .ReturnsAsync(new List<Class>
      {
           new Class
            {
                Id = 10,
                TeacherId = 2,
                EndDate = friday.AddDays(-1),
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Name = "Week 4",
                                DayOfWeek = "THURSDAY",
                                StartTime = new TimeOnly(9, 0),
                                EndTime = new TimeOnly(10, 0),
                                Date = friday.AddDays(-1)
                            }
                        }
                    }
                }
            },
                new Class
            {
                Id = 11,
                TeacherId = 2,
                EndDate = friday.AddDays(11),
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Name = "Week 6",
                                DayOfWeek = "Saturday",
                                StartTime = new TimeOnly(9, 0),
                                EndTime = new TimeOnly(10, 0),
                                Date = friday.AddDays(11)
                            }
                        }
                    }
                }
            },
                 new Class
            {
                Id = 12,
                TeacherId = 2,
                EndDate = friday.AddDays(10),
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Name = "Week 5",
                                DayOfWeek = "Friday",
                                StartTime = new TimeOnly(8, 0),
                                EndTime = new TimeOnly(9, 0),
                                Date = friday
                            }
                        }
                    }
                }
            },
                   new Class
            {
                Id = 13,
                TeacherId = 2,
                EndDate = friday,
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Name = "Week 5",
                                DayOfWeek = "Friday",
                                StartTime = new TimeOnly(9, 0),
                                EndTime = new TimeOnly(10, 0),
                                Date = friday
                            }
                        }
                    }
                }
            }
      });


            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0)),
        new("Sunday", new TimeOnly(14, 0), new TimeOnly(15, 0))
    }
   );
            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("conflict");
            result.Message.Should().Contain("already has a class at");
        }


        [Fact]
        public async Task CreateClass_ShouldReturnOkExistingInSystem()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));

            _mockSyllabusRepo.Setup(x => x.GetSyllabusByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Syllabus
                {
                    Id = 1,
                    Name = "PRN222",
                    HoursOfSyllabus = 20,
                    IsActive = true
                });
            _mockAuthClient.Setup(x => x.GetTeacherProfileDtoById(It.IsAny<int>()))
 .ReturnsAsync(new AccountDto(2, "huatri2004@gmail.com", "teacher12", "TEACHER", "ACCOUNT_ACTIVE", "0918001135", "47 PNT", DateTime.UtcNow));
            _mockClassRepo.Setup(x => x.GetClassByYearAndSyllabusId(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Class
            {
                Id = 1,
                Name = "Class01",
                AcademicYear = friday.Year,
                EndDate = friday.AddDays(-7),
                SyllabusId = 1,
                Version = 1
            });

            var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
);

            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Create class successfully");

        }

        [Fact]
        public async Task CreateClass_ShouldReturnOk()
        {
            var friday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12 - (int)DateTime.UtcNow.DayOfWeek));

            _mockSyllabusRepo.Setup(x => x.GetSyllabusByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Syllabus
                {
                    Id = 1,
                    Name = "PRN222",
                    HoursOfSyllabus = 20,
                    IsActive = true
                });

            _mockAuthClient.Setup(x => x.GetTeacherProfileDtoById(It.IsAny<int>()))
    .ReturnsAsync(new AccountDto(2, "huatri2004@gmail.com", "teacher12", "TEACHER", "ACCOUNT_ACTIVE", "0918001135", "47 PNT", DateTime.UtcNow));

            _mockClassRepo.Setup(x => x.GetExistingClassesByTeacherIdAsync(It.IsAny<int>()))
     .ReturnsAsync(new List<Class>
     {
            new Class
            {
                Id = 10,
                TeacherId = 2,
                EndDate = friday.AddDays(-1),
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Id = 4,
                        ClassesId = 10,
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Name = "Week 4",
                                DayOfWeek = "THURSDAY",
                                StartTime = new TimeOnly(9, 0),
                                EndTime = new TimeOnly(10, 0),
                                Date = friday.AddDays(-1)
                            }
                        }
                    }
                }
            },
                new Class
            {
                Id = 11,
                TeacherId = 2,
                EndDate = friday.AddDays(11),
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Id = 12,
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Id = 1,
                                Name = "Week 6",
                                DayOfWeek = "Saturday",
                                StartTime = new TimeOnly(9, 0),
                                EndTime = new TimeOnly(10, 0),
                                ScheduleId = 12,
                                Date = friday.AddDays(11)
                            }
                        }
                    }
                }
            },
                 new Class
            {
                Id = 12,
                TeacherId = 2,
                EndDate = friday.AddDays(10),
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Name = "Week 5",
                                DayOfWeek = "Friday",
                                StartTime = new TimeOnly(8, 0),
                                EndTime = new TimeOnly(9, 0),
                                Date = friday
                            }
                        }
                    }
                }
            },
                   new Class
            {
                Id = 13,
                TeacherId = 2,
                StartDate =  friday,
                Schedules = new List<Schedule>
                {
                    new Schedule
                    {
                        Activities = new List<Activity>
                        {
                            new Activity
                            {
                                Name = "Week 1",
                                DayOfWeek = "Friday",
                                StartTime = new TimeOnly(8, 0),
                                EndTime = new TimeOnly(9, 0),
                                Date = friday
                            }
                        }
                    }
                }
            },
     });
 
        var req = new CreateClassRequest(friday, 1, 2, new List<ActivityRequest>
    {
        new("Friday", new TimeOnly(9, 0), new TimeOnly(10, 0)),
        new("Friday", new TimeOnly(10, 0), new TimeOnly(11, 0)),
        new("Sunday", new TimeOnly(13, 0), new TimeOnly(14, 0))
    }
);

            var result = await _service.CreateClass(req);
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Create class successfully");
        }




        [Fact]
        public async Task GetClassById_ShouldReturnNotFound()
        {
            _mockClassRepo.Setup(x => x.GetClassByIdAsync(1))
                  .ReturnsAsync((Class?)null);

            var result = await _service.GetClassByIdAsync(1);
            result.StatusResponseCode.Should().Be("notFound");
            result.Message.Should().Contain("not found or be deleted.");
        }

        [Fact]
        public async Task GetClassById_ShouldReturnOk()
        {

      

            _mockAuthClient.Setup(x => x.GetTeacherProfileDtoById(It.IsAny<int>()))
.ReturnsAsync(new AccountDto(2, "huatri2004@gmail.com", "teacher12", "TEACHER", "ACCOUNT_ACTIVE", "0918001135", "47 PNT", DateTime.UtcNow));

            _mockClassRepo.Setup(x => x.GetClassByIdAsync(1))
                  .ReturnsAsync(new Class
                  {
                      Id = 1,
                      Name = "Old",
                      NumberStudent = 0,
                      AcademicYear = 2025,
                      NumberOfWeeks = 3,
                      StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                      EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                      Syllabus = new Syllabus
                      {
                          Id = 1,
                          Name = "SE",
                          Cost = 200000,
                          HoursOfSyllabus = 20,
                          IsActive = true
                      },

                      Status = "inactive",
                      PatternActivities = new List<PatternActivity>
                      {
                          new PatternActivity
                          {
                              Id = 1,
                              ClassId = 1,
                              DayOfWeek = "MONDAY",
                              StartTime = new TimeOnly(4,0),
                              EndTime = new TimeOnly(5,0)
                          }
                      }
                  });


            var result = await _service.GetClassByIdAsync(1);
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("successfully");
        }

        [Fact]
        public async Task GetClassesByIds_ShouldReturnBadRequest_WhenIdsIsEmpty()
        {
            // Arrange
            var ids = new List<int>();

            // Act
            var result = await _service.GetClassesByIds(ids);

            // Assert
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Ids cannot be empty");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetClassesByIds_ShouldReturnBadRequest_WhenIdsIsNull()
        {
            // Arrange
            List<int>? ids = null;

            // Act
            var result = await _service.GetClassesByIds(ids);

            // Assert
            result.StatusResponseCode.Should().Be("badRequest");
            result.Message.Should().Contain("Ids cannot be empty");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetClassesByIds_ShouldReturnOk_WhenValidIds()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };

            _mockClassRepo.Setup(x => x.GetClassesByIdsAsync(ids))
                .ReturnsAsync(new List<Class>
                {
            new Class
            {
                Id = 1,
                Name = "PRN222",
                NumberOfWeeks = 10,
                NumberStudent = 30,
                AcademicYear = 2025,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                Status = "active",
                Syllabus = new Syllabus { Id = 1, Cost = 200000 },
                PatternActivities = new List<PatternActivity>
                {
                    new PatternActivity
                    {
                        DayOfWeek = "FRIDAY",
                        StartTime = new TimeOnly(8,0),
                        EndTime = new TimeOnly(9,0)
                    }
                }
            }
                });

            // Act
            var result = await _service.GetClassesByIds(ids);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get Classes By Ids successfully");

            var data = result.Data.Should().BeAssignableTo<IEnumerable<ClassDto>>().Subject.ToList();
            data.Should().HaveCount(1);
            data[0].Name.Should().Be("PRN222");
            data[0].PatternActivitiesDTO.Should().ContainSingle(p =>
                p.DayOfWeek == "FRIDAY" && p.StartTime == new TimeOnly(8, 0));
        }

        [Fact]
        public async Task GetClassesAfterDateInYearAsync_ShouldReturnOk_WithMappedResult()
        {
            // Arrange
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)); // Ví dụ: 2025-11-04

            _mockClassRepo.Setup(x => x.GetClassesAfterDateInYearAsync(endDate, endDate.Year))
                .ReturnsAsync(new List<Class>
                {
            new Class
            {
                Id = 1,
                Name = "PRN222",
                NumberOfWeeks = 10,
                NumberStudent = 30,
                AcademicYear = endDate.Year,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
                Syllabus = new Syllabus { Id = 5, Cost = 300000 },
                Status = "inactive"
            },
            new Class
            {
                Id = 2,
                Name = "SWP391",
                NumberOfWeeks = 12,
                NumberStudent = 40,
                AcademicYear = endDate.Year,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                Syllabus = new Syllabus { Id = 6, Cost = 500000 },
                Status = "inactive"
            }
                });

            // Act
            var result = await _service.GetClassesAfterDateInYearAsync(endDate);

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain($"View list of inactive classes in {endDate.Year} successfully");

            var data = result.Data.Should().BeAssignableTo<IEnumerable<ClassDto>>().Subject.ToList();
            data.Should().HaveCount(2);
            data[0].Name.Should().Be("PRN222");
            data[0].Cost.Should().Be(300000);
            data[1].Name.Should().Be("SWP391");
            data[1].Cost.Should().Be(500000);

            // Verify repository được gọi đúng tham số
            _mockClassRepo.Verify(x => x.GetClassesAfterDateInYearAsync(endDate, endDate.Year), Times.Once);
        }

        [Fact]
        public async Task GetAllClassesAsync_ShouldReturnOk_WhenClassesExist()
        {
            // Arrange
            var classList = new List<Class>
    {
        new Class
        {
            Id = 1,
            Name = "PRN222",
            NumberOfWeeks = 10,
            NumberStudent = 30,
            AcademicYear = 2025,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            Status = "active",
            TeacherId = 2,
            Syllabus = new Syllabus
            {
                Id = 1,
                Name = "Software Engineering",
                Cost = 200000
            }
        }
    };

            _mockClassRepo.Setup(x => x.GetClassesAsync())
                .ReturnsAsync(classList);

            _mockAuthClient.Setup(x => x.GetTeacherProfileDtoById(2))
                .ReturnsAsync(new AccountDto(
                    2,
                    "teacher@gmail.com",
                    "Nguyen Van A",
                    "TEACHER",
                    "ACCOUNT_ACTIVE",
                    "0909000000",
                    "HCM",
                    DateTime.UtcNow
                ));

            // Act
            var result = await _service.GetAllClassesAsync();

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get All classes successfully");

            var data = result.Data.Should().BeAssignableTo<IEnumerable<ClassDto>>().Subject.ToList();
            data.Should().HaveCount(1);
            data[0].Name.Should().Be("PRN222");
            data[0].teacherName.Should().Be("Nguyen Van A");
            data[0].teacherEmail.Should().Be("teacher@gmail.com");
            data[0].Cost.Should().Be(200000);
        }

        [Fact]
        public async Task GetAllClassesAsync_ShouldHandleNullTeacherAndNullSyllabus()
        {
            // Arrange
            var classList = new List<Class>
    {
        new Class
        {
            Id = 10,
            Name = "PRN231",
            NumberOfWeeks = 12,
            NumberStudent = 20,
            AcademicYear = 2025,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = "inactive",
            TeacherId = 999,
            Syllabus = null // cost fallback 0
        }
    };

            _mockClassRepo.Setup(x => x.GetClassesAsync())
                .ReturnsAsync(classList);

            // Simulate teacher not found
            _mockAuthClient.Setup(x => x.GetTeacherProfileDtoById(999))
                .ReturnsAsync((AccountDto?)null);

            // Act
            var result = await _service.GetAllClassesAsync();

            // Assert
            result.StatusResponseCode.Should().Be("ok");
            result.Message.Should().Contain("Get All classes successfully");

            var data = result.Data.Should().BeAssignableTo<IEnumerable<ClassDto>>().Subject.ToList();
            data.Should().HaveCount(1);
            data[0].teacherName.Should().Be("Unknown");
            data[0].teacherEmail.Should().Be("N/A");
            data[0].Cost.Should().Be(0);
        }


    }
}
