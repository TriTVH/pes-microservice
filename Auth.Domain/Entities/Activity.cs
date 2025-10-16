
    using System;
    using System.Collections.Generic;

    namespace Auth.Domain.Entities
    {


        public class Activity
        {
            public int Id { get; set; }

            public DateOnly? Date { get; set; }

            public string? DayOfWeek { get; set; } 

            public TimeOnly? EndTime { get; set; }

            public string Name { get; set; }

            public TimeOnly? StartTime { get; set; }


            public int? ScheduleId { get; set; }

            public virtual Schedule Schedule { get; set; }
        }
    }