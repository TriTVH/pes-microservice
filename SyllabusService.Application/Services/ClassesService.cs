using Microsoft.IdentityModel.Tokens;
using SyllabusService.Application.DTOs.Request;
using SyllabusService.Application.DTOs.Response;
using SyllabusService.Application.Services.IServices;
using SyllabusService.Domain.IClient;
using SyllabusService.Infrastructure.Models;
using SyllabusService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.DTOs;
using static Azure.Core.HttpHeader;

namespace SyllabusService.Application.Services
{
    public class ClassesService : IClassesServices
    {
        private ISyllabusRepository _syllabusRepo;
        private IClassRepository _classRepo;
        private IAuthClient _authClient;
        public ClassesService(ISyllabusRepository syllabusRepo, IClassRepository classRepo, IAuthClient authClient)
        {
            _syllabusRepo = syllabusRepo;
            _classRepo = classRepo;
            _authClient = authClient;
        }

        public async Task<ResponseObject> CreateClass(CreateClassRequest request)
        {

            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                   ? "SE Asia Standard Time"
                   : "Asia/Ho_Chi_Minh";

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            string error = ValidateCreateClass(request);
            if (!error.IsNullOrEmpty())
            {
                return new ResponseObject("badRequest", error, null);
            }

            Syllabus syllabus = await _syllabusRepo.GetSyllabusByIdAsync(request.syllabusId);

            if (syllabus == null)
            {
                return new ResponseObject("notFound", "Syllabus not found or be deleted", null);
            }

            var teacherDto = await _authClient.GetTeacherProfileDtoById(request.teacherId);

            if(teacherDto == null)
            {
                return new ResponseObject("notFound", "Teacher not found or be deleted", null);
            }

            var existingClasses = await _classRepo.GetClassesByTeacherIdAsync(request.teacherId);

            foreach (var existingClass in existingClasses)
            {
  
                if (existingClass.EndDate < request.startDate)
                    continue;

                foreach (var schedule in existingClass.Schedules)
                {
                    foreach (var act in schedule.Activities)
                    {
                        foreach (var reqActivity in request.activities)
                        {
                            Enum.TryParse<DayOfWeek>(reqActivity.dayOfWeek, true, out var reqDayOfWeek);

                            bool isSameDay = string.Equals(act.DayOfWeek, reqDayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase);
                            bool isSameTime = act.StartTime == reqActivity.startTime;

                            // So sánh ngày diễn ra có bị trùng không (ví dụ nếu lớp đang diễn ra cùng thời gian)
                            bool isDateConflict = act.Date >= request.startDate;

                            if (isSameDay && isSameTime && isDateConflict)
                            {
                                return new ResponseObject("conflict",
                                    $"Teacher {teacherDto.Name} already has a class at {reqActivity.dayOfWeek} {reqActivity.startTime} on {act.Date}", null);
                            }
                        }
                    }
                }
            }

            Class existingInSystem = await _classRepo.GetClassByYearAndSyllabusId(request.startDate.Year, request.syllabusId);

            Class classes = new();

            if (existingInSystem == null)
            {
                classes.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                classes.AcademicYear = request.startDate.Year;
                classes.Name = $"Class {syllabus.Name}_{request.startDate.Year}_v1";
                classes.StartDate = request.startDate;
                classes.Status = "inactive";
                classes.TeacherId = request.teacherId;
                classes.Version = 1;
                classes.SyllabusId = request.syllabusId;
                classes.NumberStudent = 0;
            } else {
                classes.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                classes.Version = existingInSystem.Version + 1;
                classes.SyllabusId = request.syllabusId;
                classes.AcademicYear = request.startDate.Year;
                classes.Name = $"Class {syllabus.Name}_{request.startDate.Year}_v{existingInSystem.Version + 1}";
                classes.StartDate = request.startDate;
                classes.TeacherId = request.teacherId;
                classes.Status = "inactive";
                classes.NumberStudent = 0;
            }
            List<PatternActivity> patternActivities = new List<PatternActivity>();

            List<Schedule> schedules = new List<Schedule>();

            foreach (var item in request.activities)
            {
              if (!Enum.TryParse<DayOfWeek>(item.dayOfWeek, true, out var parsedDay))
                    throw new ArgumentException($"Invalid dayOfWeek: {item.dayOfWeek}");

                patternActivities.Add(new PatternActivity
                {
                    DayOfWeek = parsedDay.ToString(),
                    StartTime = item.startTime,
                    EndTime = item.endTime,
                });
            }

            DateOnly currentDate = request.startDate;

            int totalHoursAdded = 0;

            int weekIndex = 1;

            while (totalHoursAdded < syllabus.HoursOfSyllabus)
            {
                int remainingHours = syllabus.HoursOfSyllabus - totalHoursAdded;

                int hoursThisWeek = Math.Min(4, remainingHours); 

                int hoursAddedThisWeek = 0;

                List<Activity> activitiesThisWeek = new();

                foreach (var pattern in patternActivities)
                {
                    if (hoursAddedThisWeek >= hoursThisWeek || totalHoursAdded >= syllabus.HoursOfSyllabus)
                        break;

                    DayOfWeek targetDay = Enum.Parse<DayOfWeek>(pattern.DayOfWeek, true);
                    DateOnly nextDate = GetNextDateForDayOfWeek(currentDate, targetDay);

                    activitiesThisWeek.Add(new Activity
                    {
                        Name = classes.Name,
                        DayOfWeek = pattern.DayOfWeek,
                        StartTime = pattern.StartTime,
                        EndTime = pattern.EndTime,
                        Date = nextDate
                    });
                    hoursAddedThisWeek++;
                    totalHoursAdded++;
                }
                if (activitiesThisWeek.Count > 0)
                {
                    var (start, end) = GetWeekRange(currentDate);
                    schedules.Add(new Schedule
                    {
                        Activities = activitiesThisWeek,
                        StartDate = start,
                        EndDate = end,
                        WeekName = $"Week - {weekIndex}"
                    });
                    weekIndex++;
                }
                currentDate = currentDate.AddDays(7); // sang tuần kế tiếp
            }
            classes.PatternActivities = patternActivities;
            classes.NumberOfWeeks = weekIndex;
            
            var lastSchedule = schedules.LastOrDefault();

            if (lastSchedule != null)
            {
            
                var lastActivity = lastSchedule.Activities
                    .OrderByDescending(a => Enum.Parse<DayOfWeek>(a.DayOfWeek))
                    .FirstOrDefault();

                if (lastActivity != null)
                {
                    classes.EndDate = lastActivity.Date; // Gán EndDate cho lớp học
                }
            }
            classes.Schedules = schedules;
            await _classRepo.CreateClassAsync(classes);
            return new ResponseObject("ok", "Create class successfully", null);
        }


        public async Task<ResponseObject> GetAllClassesAsync()
        {
            var items = await _classRepo.GetClassesAsync();
            var result = items.Select(c => new ClassDto()
            {
                Id = c.Id,
                Name = c.Name,
                NumberOfWeeks = c.NumberOfWeeks,
                NumberStudent = c.NumberStudent,
                AcademicYear = c.AcademicYear,
                StartDate = c.StartDate,
                Cost = c.Syllabus.Cost,
                Status = c.Status
            });

            return new ResponseObject("ok", "Get All classes successfully", result);
        }

        public async Task<ResponseObject> GetClassesByIds(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new ResponseObject("badRequest", "Ids cannot be empty", null);
            }
            var classes = await _classRepo.GetClassesByIdsAsync(ids);

            var result = classes.Select(c => new ClassDto()
            {
                Id = c.Id,
                Name = c.Name,
                NumberOfWeeks = c.NumberOfWeeks,
                NumberStudent = c.NumberStudent,
                AcademicYear = c.AcademicYear,
                StartDate = c.StartDate,
                Cost = c.Syllabus.Cost,
                Status = c.Status,
                PatternActivitiesDTO = c.PatternActivities.Select(pa => new PatternActivityDto()
                {
                    DayOfWeek = pa.DayOfWeek,
                    StartTime = pa.StartTime,
                    EndTime = pa.EndTime
                }).ToList()
            });
            return new ResponseObject("ok", "Get Classes By Ids successfully", result);
        }
        public string? ValidateCreateClass(CreateClassRequest request)
        {
           
            if (request.startDate < DateOnly.FromDateTime(DateTime.Now))
            {
                return "Start date cannot be in the past.";
            }
            if(request.activities.Count <= 0)
            {
                return "Please add at least one activity to create class";
            }
            if (request.activities.Count > 4)
            {
                return "You can add up to 4 activities only.";
            }

            var comboSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var parsedDays = new List<DayOfWeek>();

            var hoursPerDay = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);


            for (int i = 0; i < request.activities.Count; i++)
            {
                var activity = request.activities[i];

                if (!Enum.TryParse(activity.dayOfWeek, true, out DayOfWeek parsedDay))
                {
                    return $"Invalid dayOfWeek value: '{activity.dayOfWeek}'. Must be one of Sunday, Monday, ..., Saturday.";
                }

                if (parsedDays.Count == 0)
                {
                    if (parsedDay != request.startDate.DayOfWeek)
                    {
                        return $"Start date must match the dayOfWeek of the first activity. Expected '{request.startDate.DayOfWeek}', got '{parsedDay}'.";
                    }
                }
                else
                {
                    var prevDay = parsedDays.Last();

                    // Kiểm tra thứ tự tăng dần
                    if ((int)parsedDay < (int)prevDay)
                    {
                        return $"DayOfWeek of activity #{parsedDays.Count + 1} ('{parsedDay}') must be after previous one ('{prevDay}').";
                    }
                }

                // Kiểm tra không có ngày nào trước startDate
                if ((int)parsedDay < (int)request.startDate.DayOfWeek)
                {
                    return $"Activity '{activity.dayOfWeek}' must be on or after the startDate's dayOfWeek: '{request.startDate.DayOfWeek}'.";
                }

                parsedDays.Add(parsedDay);


            string comboKey = $"{activity.dayOfWeek}_{activity.startTime}_{activity.endTime}";
                if (!comboSet.Add(comboKey))
                {
                    return $"Duplicate activity combo detected: {comboKey}. Each (dayOfWeek, startTime, endTime) must be unique.";
                }

        
                if (!IsFullHour(activity.startTime) || !IsFullHour(activity.endTime))
                {
                    return "StartTime and EndTime must be in full hour format like '07:00', '08:00'.";
                }


                if (activity.endTime <= activity.startTime)
                {
                    return "EndTime must be after StartTime.";
                }

               
                if ((activity.endTime.ToTimeSpan() - activity.startTime.ToTimeSpan()) != TimeSpan.FromHours(1))
                {
                    return "Each activity must last exactly 1 hour.";
                }

                if (!hoursPerDay.ContainsKey(activity.dayOfWeek))
                {
                    hoursPerDay[activity.dayOfWeek] = 0;
                }

                hoursPerDay[activity.dayOfWeek] += 1;

                if (hoursPerDay[activity.dayOfWeek] > 2)
                {
                    return $"Each dayOfWeek can have at most 2 hours of activities. '{activity.dayOfWeek}' has more than 2.";
                }
            }

            return "";
        }
        private bool IsFullHour(TimeOnly time)
        {
            return time.Minute == 0 && time.Second == 0;
        }
        private DateOnly GetNextDateForDayOfWeek(DateOnly fromDate, DayOfWeek targetDay)
        {
            int currentDay = (int)fromDate.DayOfWeek;
            int target = (int)targetDay;

            int daysToAdd = (target - currentDay + 7) % 7;
            return fromDate.AddDays(daysToAdd);
        }

        public (DateTime StartOfWeek, DateTime EndOfWeek) GetWeekRange(DateOnly date)
        {
            // Chuyển DateOnly → DateTime (với giờ mặc định là 00:00)
            DateTime dateTime = date.ToDateTime(new TimeOnly(0, 0));

            // Tính thứ mấy trong tuần (Monday = 1, Sunday = 0)
            int diffToMonday = ((int)date.DayOfWeek + 6) % 7;

            // Tính ngày đầu tuần (thứ 2)
            DateTime startOfWeek = dateTime.AddDays(-diffToMonday).Date;

            // Ngày cuối tuần (chủ nhật)
            DateTime endOfWeek = startOfWeek.AddDays(6);

            return (startOfWeek, endOfWeek);
        }

        public async Task<ResponseObject> GetClassesAfterDateInYearAsync(DateOnly endDate)
        {
  
            var items = await _classRepo.GetClassesAfterDateInYearAsync(endDate, endDate.Year);
            var result = items.Select(cla => new ClassDto()
            {
                Id = cla.Id,
                Name = cla.Name,
                NumberOfWeeks = cla.NumberOfWeeks,
                NumberStudent = cla.NumberStudent,
                StartDate = cla.StartDate,
                AcademicYear = cla.AcademicYear,
                Cost = cla.Syllabus.Cost,
                Status = cla.Status
            });
            return new ResponseObject("ok", $"View list of inactive classes in {endDate.Year} successfully", result);
        }

        public async Task<ResponseObject> CheckClassesAvailability(CheckClassRequest request)
        {
           
            if (request.CurrentClassId <= 0 || request.StudentId <= 0)
            {
                return new ResponseObject("badRequest", "Invalid request data.", null);
            }

        
            var checkedIds = request.CheckedClassIds?.ToList() ?? new List<int>();

            if (checkedIds.Contains(request.CurrentClassId))
            {
                return new ResponseObject(
                    "conflict",
                    $"Class with ID {request.CurrentClassId} is already selected.",
                    checkedIds
                );
            }


            var newClass = await _classRepo.GetClassByIdAsync(request.CurrentClassId);

            if (newClass == null)
                return new ResponseObject("notFound", $"Class with ID {request.CurrentClassId} not found.", null);

            var checkedClasses = await _classRepo.GetClassesByIdsAsync(checkedIds);
         
            if (checkedClasses == null || !checkedClasses.Any())
            {
                checkedIds.Add(request.CurrentClassId);
                return new ResponseObject("ok", "No existing classes found — no conflict detected.", checkedIds);
            }
           
            foreach (var existingClass in checkedClasses)
            {
                // ❗ Bỏ qua nếu thời gian học không giao nhau (theo StartDate – EndDate)
                if (newClass.StartDate > existingClass.EndDate || existingClass.StartDate > newClass.EndDate)
                    continue;

                foreach (var actA in newClass.PatternActivities)
                {
                    foreach (var actB in existingClass.PatternActivities)
                    {
                        if (actA.DayOfWeek == actB.DayOfWeek &&
                            actA.StartTime < actB.EndTime &&
                            actB.StartTime < actA.EndTime)
                        {
                            var conflictMessage =
                                $"Conflict detected: Class '{newClass.Name}' (ID {newClass.Id}) " +
                                $"conflicts with already selected Class '{existingClass.Name}' (ID {existingClass.Id}) " +
                                $"on {actA.DayOfWeek} " +
                                $"({actA.StartTime:hh\\:mm} - {actA.EndTime:hh\\:mm}).";

                            return new ResponseObject("conflict", conflictMessage, checkedIds);
                        }
                    }
                }
            }
            checkedIds.Add(request.CurrentClassId);
            return new ResponseObject("ok", "No schedule conflicts detected.", checkedIds);
        }

        public async Task<ResponseObject> GetClassByIdAsync(int id)
        {
            var classes = await _classRepo.GetClassByIdAsync(id);
            
            if (classes == null)
            {
                return new ResponseObject(
                           "notFound",
                           $"Class with ID {id} not found or be deleted.",
                           null
                       );
            }

            var result = new ClassDto()
            {
                Id = id,
                Name = classes.Name,
                NumberStudent = classes.NumberStudent,
                AcademicYear = classes.AcademicYear,
                NumberOfWeeks = classes.NumberOfWeeks,
                StartDate = classes.StartDate,
                Cost = classes.Syllabus.Cost,
                Status = classes.Status
            };

            return new ResponseObject(
            "ok",
            "Class found successfully.",
            result
            );
        }

    }
}
