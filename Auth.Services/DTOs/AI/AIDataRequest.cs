using System;
using System.Collections.Generic;

namespace Auth.Services.DTOs.AI
{
    public class AIDataRequest
    {
        public string RequestType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string UserRole { get; set; } = "GUEST";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AIDataResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // Các loại request AI có thể thực hiện
    public static class AIRequestTypes
    {
        public const string GET_CLASS_BY_NAME = "get_class_by_name";
        public const string GET_CLASSES_BY_STATUS = "get_classes_by_status";
        public const string GET_TEACHER_BY_EMAIL = "get_teacher_by_email";
        public const string GET_TEACHERS_BY_ROLE = "get_teachers_by_role";
        public const string GET_PARENT_BY_JOB = "get_parent_by_job";
        public const string GET_PARENTS_BY_RELATIONSHIP = "get_parents_by_relationship";
        public const string GET_ACTIVITIES_BY_CLASS = "get_activities_by_class";
        public const string GET_SCHEDULES_BY_WEEK = "get_schedules_by_week";
        public const string GET_STATISTICS_DETAILED = "get_statistics_detailed";
        public const string SEARCH_ACCOUNTS = "search_accounts";
        public const string GET_CLASS_CAPACITY = "get_class_capacity";
    }
}
