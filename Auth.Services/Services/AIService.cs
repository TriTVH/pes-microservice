using Auth.Services.DTOs.AI;
using Auth.Services.Services.IServices;
using Auth.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Auth.Services.Services
{
    public class AIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIService> _logger;
        private readonly IAccountRepository _accountRepository;
        private readonly IParentRepository _parentRepository;
        private readonly ITeacherActionRepository _teacherRepository;
        private readonly IClassRepository _classRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly string _projectId;

        public AIService(
            IConfiguration configuration, 
            ILogger<AIService> logger,
            IAccountRepository accountRepository,
            IParentRepository parentRepository,
            ITeacherActionRepository teacherRepository,
            IClassRepository classRepository,
            IScheduleRepository scheduleRepository,
            IActivityRepository activityRepository,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _accountRepository = accountRepository;
            _parentRepository = parentRepository;
            _teacherRepository = teacherRepository;
            _classRepository = classRepository;
            _scheduleRepository = scheduleRepository;
            _activityRepository = activityRepository;
            _httpClient = httpClient;
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
            _baseUrl = _configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com";
            _model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
            _projectId = _configuration["Gemini:ProjectId"] ?? "pes-microservice";
        }

        public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request)
        {
            try
            {
                var userMessage = request.Message;
                var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
                var userRole = request.UserRole ?? "GUEST";

                // 1. Phân tích câu hỏi để xác định cần data gì
                var dataContext = await AnalyzeAndGetDataContext(userMessage, userRole);
                
                // 2. Kiểm tra xem AI có cần thêm data không (Dynamic Data Fetching)
                var additionalData = await GetAdditionalDataIfNeeded(userMessage, userRole);
                if (additionalData != null)
                {
                    dataContext.AdditionalData = additionalData;
                }
                
                // 3. Lấy system prompt với data context
                var systemPrompt = await GetSystemPromptWithDataAsync(dataContext, userRole);
                
                // 3. Tạo full prompt với data
                var fullPrompt = $"{systemPrompt}\n\nUser Question: {userMessage}";

                // 4. Call Gemini API
                var response = await CallGeminiAPIAsync(fullPrompt);
                
                // 5. Xử lý markdown và ký tự đặc biệt
                response = ProcessMarkdownAndSpecialChars(response);

                // 6. Extract related topics và suggested actions
                var relatedTopics = ExtractRelatedTopics(userMessage);
                var suggestedActions = GenerateSuggestedActions(userMessage, userRole);

                return new ChatResponseDto
                {
                    Response = response,
                    SessionId = sessionId,
                    Timestamp = DateTime.UtcNow,
                    SuggestedActions = suggestedActions,
                    RelatedTopics = relatedTopics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI chat service");
                return new ChatResponseDto
                {
                    Response = "Xin lỗi, tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau.",
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    SuggestedActions = "Thử lại sau hoặc liên hệ hỗ trợ kỹ thuật",
                    RelatedTopics = new List<string> { "Hỗ trợ kỹ thuật", "Lỗi hệ thống" }
                };
            }
        }

        private async Task<string> CallGeminiAPIAsync(string prompt)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 1024
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/v1beta/models/{_model}:generateContent?key={_apiKey}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
                    
                    if (geminiResponse?.candidates?.Length > 0)
                    {
                        return geminiResponse.candidates[0].content.parts[0].text;
                    }
                }

                return "Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return "Xin lỗi, có lỗi xảy ra khi kết nối với AI service.";
            }
        }

        public async Task<string> GetSystemPromptAsync()
        {
            // Sử dụng method mới với data context rỗng
            return await GetSystemPromptWithDataAsync(new DataContext(), "GUEST");
        }

        // Test method để kiểm tra database connection
        public async Task<string> TestDatabaseConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing database connection...");
                
                // Test 1: Get statistics
                var stats = await GetStatisticsData();
                _logger.LogInformation($"Statistics: {stats.TotalAccounts} accounts, {stats.TotalClasses} classes");
                
                // Test 2: Get classes
                var classes = await GetClassesData();
                _logger.LogInformation($"Classes: {classes.Count} classes found");
                
                // Test 3: Get teachers
                var teachers = await GetTeachersData();
                _logger.LogInformation($"Teachers: {teachers.Count} teachers found");
                
                // Test 4: Get parents
                var parents = await GetParentsData();
                _logger.LogInformation($"Parents: {parents.Count} parents found");
                
                return $"Database connection test successful! Found: {stats.TotalAccounts} accounts, {stats.TotalClasses} classes, {teachers.Count} teachers, {parents.Count} parents";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed: {ErrorMessage}", ex.Message);
                return $"Database connection test failed: {ex.Message}";
            }
        }

        private List<string> ExtractRelatedTopics(string message)
        {
            var topics = new List<string>();
            var lowerMessage = message.ToLower();

            if (lowerMessage.Contains("đăng ký") || lowerMessage.Contains("register"))
                topics.Add("Đăng ký tài khoản");
            
            if (lowerMessage.Contains("đăng nhập") || lowerMessage.Contains("login"))
                topics.Add("Đăng nhập");
            
            if (lowerMessage.Contains("mật khẩu") || lowerMessage.Contains("password"))
                topics.Add("Quản lý mật khẩu");
            
            if (lowerMessage.Contains("phụ huynh") || lowerMessage.Contains("parent"))
                topics.Add("Quản lý phụ huynh");
            
            if (lowerMessage.Contains("giáo viên") || lowerMessage.Contains("teacher"))
                topics.Add("Quản lý giáo viên");
            
            if (lowerMessage.Contains("học sinh") || lowerMessage.Contains("student"))
                topics.Add("Quản lý học sinh");
            
            if (lowerMessage.Contains("lớp học") || lowerMessage.Contains("class"))
                topics.Add("Quản lý lớp học");
            
            if (lowerMessage.Contains("lịch học") || lowerMessage.Contains("schedule"))
                topics.Add("Quản lý lịch học");
            
            if (lowerMessage.Contains("chương trình") || lowerMessage.Contains("syllabus"))
                topics.Add("Chương trình học");
            
            if (lowerMessage.Contains("thanh toán") || lowerMessage.Contains("payment"))
                topics.Add("Thanh toán học phí");

            return topics;
        }

        private string? GenerateSuggestedActions(string message, string? userRole)
        {
            var lowerMessage = message.ToLower();
            var actions = new List<string>();

            // Phân tích tính cách và sở thích
            if (lowerMessage.Contains("năng động") || lowerMessage.Contains("thể thao") || lowerMessage.Contains("hoạt động"))
            {
                actions.Add("Xem mục 'Chương trình học' để tìm lớp có hoạt động thể thao");
                actions.Add("Tham khảo mục 'Hoạt động ngoại khóa'");
                actions.Add("Liên hệ giáo viên thể dục để tư vấn");
            }

            if (lowerMessage.Contains("học kém") || lowerMessage.Contains("cải thiện") || lowerMessage.Contains("hỗ trợ học"))
            {
                actions.Add("Xem mục 'Hoạt động học tập' để tìm bài tập bổ sung");
                actions.Add("Liên hệ giáo viên bộ môn để được hướng dẫn");
                actions.Add("Trao đổi với giáo viên chủ nhiệm về tình hình học tập");
            }

            if (lowerMessage.Contains("lớp nào") || lowerMessage.Contains("phù hợp") || lowerMessage.Contains("tính cách"))
            {
                actions.Add("Xem mục 'Danh sách lớp học' để tìm lớp phù hợp");
                actions.Add("Tham khảo mục 'Chương trình học' để hiểu nội dung từng lớp");
                actions.Add("Trao đổi với giáo viên chủ nhiệm để được tư vấn");
            }

            // Các chức năng cơ bản
            if (lowerMessage.Contains("đăng ký"))
            {
                if (userRole == "HR")
                    actions.Add("Liên hệ bộ phận nhân sự để tạo tài khoản giáo viên");
                else
                    actions.Add("Truy cập trang web PES và nhấn 'Đăng ký phụ huynh'");
            }

            if (lowerMessage.Contains("đăng nhập"))
                actions.Add("Truy cập trang web PES và nhấn 'Đăng nhập'");

            if (lowerMessage.Contains("mật khẩu"))
            {
                actions.Add("Nhấn 'Quên mật khẩu' trên trang đăng nhập");
                actions.Add("Kiểm tra email để nhận link reset");
            }

            if (lowerMessage.Contains("lớp học") && userRole == "TEACHER")
                actions.Add("Đăng nhập và vào mục 'Lớp học của tôi'");

            if (lowerMessage.Contains("lịch học") && userRole == "TEACHER")
                actions.Add("Đăng nhập và vào mục 'Lịch dạy'");

            if (lowerMessage.Contains("thanh toán"))
                actions.Add("Đăng nhập tài khoản phụ huynh và vào mục 'Thanh toán'");

            // Gợi ý chung cho mọi tình huống
            if (actions.Count == 0)
            {
                actions.Add("Liên hệ giáo viên chủ nhiệm để được tư vấn chi tiết");
                actions.Add("Tham khảo mục 'Hỗ trợ' để tìm thông tin liên quan");
            }

            return actions.Count > 0 ? string.Join("; ", actions) : null;
        }

        // ===== NEW METHODS FOR DATABASE INTEGRATION AND SPECIAL CHARACTERS =====

        private async Task<DataContext> AnalyzeAndGetDataContext(string message, string userRole)
        {
            var dataContext = new DataContext();
            var lowerMessage = message.ToLower();

            try
            {
                _logger.LogInformation($"Analyzing message: {message} for user role: {userRole}");

                // Phân tích câu hỏi để xác định cần data gì
                if (lowerMessage.Contains("lớp học") || lowerMessage.Contains("class"))
                {
                    _logger.LogInformation("Fetching classes data...");
                    dataContext.Classes = await GetClassesData();
                    _logger.LogInformation($"Fetched {dataContext.Classes.Count} classes");
                }

                if (lowerMessage.Contains("giáo viên") || lowerMessage.Contains("teacher"))
                {
                    _logger.LogInformation("Fetching teachers data...");
                    dataContext.Teachers = await GetTeachersData();
                    _logger.LogInformation($"Fetched {dataContext.Teachers.Count} teachers");
                }

                if (lowerMessage.Contains("phụ huynh") || lowerMessage.Contains("parent"))
                {
                    _logger.LogInformation("Fetching parents data...");
                    dataContext.Parents = await GetParentsData();
                    _logger.LogInformation($"Fetched {dataContext.Parents.Count} parents");
                }

                if (lowerMessage.Contains("lịch học") || lowerMessage.Contains("schedule"))
                {
                    _logger.LogInformation("Fetching schedules data...");
                    dataContext.Schedules = await GetSchedulesData();
                    _logger.LogInformation($"Fetched {dataContext.Schedules.Count} schedules");
                }

                if (lowerMessage.Contains("hoạt động") || lowerMessage.Contains("activity"))
                {
                    _logger.LogInformation("Fetching activities data...");
                    dataContext.Activities = await GetActivitiesData();
                    _logger.LogInformation($"Fetched {dataContext.Activities.Count} activities");
                }

                if (lowerMessage.Contains("thống kê") || lowerMessage.Contains("statistics"))
                {
                    _logger.LogInformation("Fetching statistics data...");
                    dataContext.Statistics = await GetStatisticsData();
                    _logger.LogInformation($"Fetched statistics: {dataContext.Statistics.TotalAccounts} accounts, {dataContext.Statistics.TotalClasses} classes");
                }

                // Lấy data theo vai trò người dùng
                if (userRole == "PARENT")
                {
                    dataContext.UserSpecificData = await GetParentSpecificData();
                }
                else if (userRole == "TEACHER")
                {
                    dataContext.UserSpecificData = await GetTeacherSpecificData();
                }
                else if (userRole == "HR")
                {
                    dataContext.UserSpecificData = await GetHRSpecificData();
                }

                _logger.LogInformation("Data context analysis completed successfully");
                return dataContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data context");
                return dataContext; // Return empty context if error
            }
        }

        private async Task<List<ClassData>> GetClassesData()
        {
            try
            {
                _logger.LogInformation("Starting to fetch classes data from repository...");
                var classes = await _classRepository.GetClassesWithSchedulesAsync(10);
                _logger.LogInformation($"Repository returned {classes.Count()} classes");

                var result = classes.Select(c => new ClassData
                {
                    Id = c.Id,
                    Name = c.Name,
                    TeacherId = c.TeacherId ?? 0,
                    MaxStudents = c.NumberStudent ?? 0,
                    CurrentStudents = 0, // This field doesn't exist in Class entity
                    IsActive = c.Status == "ACTIVE",
                    ScheduleCount = c.Schedules?.Count ?? 0,
                    ActivityCount = c.Schedules?.SelectMany(s => s.Activities).Count() ?? 0
                }).ToList();

                _logger.LogInformation($"Processed {result.Count} classes data successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classes data: {ErrorMessage}", ex.Message);
                return new List<ClassData>();
            }
        }

        private async Task<List<TeacherData>> GetTeachersData()
        {
            try
            {
                var teachers = await _accountRepository.GetAccountsByRoleAsync("TEACHER", 10);

                return teachers.Select(t => new TeacherData
                {
                    Id = t.Id,
                    Name = t.Name,
                    Email = t.Email,
                    IsActive = t.Status == "ACCOUNT_ACTIVE"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teachers data");
                return new List<TeacherData>();
            }
        }

        private async Task<List<ParentData>> GetParentsData()
        {
            try
            {
                var parents = await _parentRepository.GetParentsWithLimitAsync(10);

                // Get account information separately to avoid circular dependency
                var accountIds = parents.Where(p => p.AccountId.HasValue).Select(p => p.AccountId.Value).ToList();
                var accounts = await _accountRepository.GetAccountsByIdsAsync(accountIds);

                return parents.Select(p => 
                {
                    var account = accounts.FirstOrDefault(a => a.Id == p.AccountId);
                    return new ParentData
                    {
                        Id = p.Id,
                        Name = account?.Name ?? "N/A",
                        Email = account?.Email ?? "N/A",
                        Job = p.Job,
                        RelationshipToChild = p.RelationshipToChild
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parents data");
                return new List<ParentData>();
            }
        }

        private async Task<List<ScheduleData>> GetSchedulesData()
        {
            try
            {
                var schedules = await _scheduleRepository.GetSchedulesWithDetailsAsync(10);

                return schedules.Select(s => new ScheduleData
                {
                    Id = s.Id,
                    ClassName = s.Classes?.Name ?? "N/A",
                    DayOfWeek = s.WeekName ?? "N/A",
                    StartTime = TimeSpan.Zero, 
                    EndTime = TimeSpan.Zero,   
                    ActivityCount = s.Activities?.Count ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedules data");
                return new List<ScheduleData>();
            }
        }

        private async Task<List<ActivityData>> GetActivitiesData()
        {
            try
            {
                var activities = await _activityRepository.GetActivitiesWithDetailsAsync(10);

                return activities.Select(a => new ActivityData
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.DayOfWeek ?? "N/A", // Using DayOfWeek as Description since Description doesn't exist
                    ClassName = a.Schedule?.Classes?.Name ?? "N/A",
                    StartTime = a.StartTime?.ToTimeSpan() ?? TimeSpan.Zero,
                    EndTime = a.EndTime?.ToTimeSpan() ?? TimeSpan.Zero
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities data");
                return new List<ActivityData>();
            }
        }

        private async Task<StatisticsData> GetStatisticsData()
        {
            try
            {
                var totalAccounts = await _accountRepository.GetTotalAccountsCountAsync();
                var totalParents = await _parentRepository.GetTotalParentsCountAsync();
                var totalClasses = await _classRepository.GetTotalClassesCountAsync();
                var totalSchedules = await _scheduleRepository.GetTotalSchedulesCountAsync();
                var totalActivities = await _activityRepository.GetTotalActivitiesCountAsync();
                
                // Thêm thống kê roles
                var teacherCount = await _accountRepository.GetAccountsCountByRoleAsync("TEACHER");
                var parentAccountCount = await _accountRepository.GetAccountsCountByRoleAsync("PARENT");
                var activeClassesCount = await _classRepository.GetActiveClassesCountAsync();

                return new StatisticsData
                {
                    TotalAccounts = totalAccounts,
                    TotalParents = totalParents,
                    TotalClasses = totalClasses,
                    TotalSchedules = totalSchedules,
                    TotalActivities = totalActivities,
                    TeacherCount = teacherCount,
                    ParentAccountCount = parentAccountCount,
                    ActiveClassesCount = activeClassesCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics data");
                return new StatisticsData();
            }
        }

        private async Task<object> GetParentSpecificData()
        {
            return new
            {
                Message = "Dữ liệu dành riêng cho phụ huynh",
                AvailableFeatures = new[] { "Quản lý con em", "Đăng ký nhập học", "Thanh toán học phí" }
            };
        }

        private async Task<object> GetTeacherSpecificData()
        {
            return new
            {
                Message = "Dữ liệu dành riêng cho giáo viên",
                AvailableFeatures = new[] { "Quản lý lớp học", "Lịch dạy", "Hoạt động học tập" }
            };
        }

        private async Task<object> GetHRSpecificData()
        {
            return new
            {
                Message = "Dữ liệu dành riêng cho nhân sự",
                AvailableFeatures = new[] { "Quản lý tài khoản", "Tuyển dụng", "Báo cáo" }
            };
        }

        // ===== DYNAMIC DATA FETCHING =====
        
        private async Task<object?> GetAdditionalDataIfNeeded(string message, string userRole)
        {
            try
            {
                var lowerMessage = message.ToLower();
                
                // Phân tích câu hỏi để xác định cần data gì
                if (lowerMessage.Contains("lớp") && lowerMessage.Contains("tên"))
                {
                    return await GetClassByNameData(message);
                }
                
                if (lowerMessage.Contains("giáo viên") && (lowerMessage.Contains("email") || lowerMessage.Contains("gmail")))
                {
                    return await GetTeacherByEmailData(message);
                }
                
                if (lowerMessage.Contains("phụ huynh") && (lowerMessage.Contains("nghề") || lowerMessage.Contains("công việc")))
                {
                    return await GetParentsByJobData(message);
                }
                
                if (lowerMessage.Contains("phụ huynh") && (lowerMessage.Contains("cha") || lowerMessage.Contains("mẹ")))
                {
                    return await GetParentsByRelationshipData(message);
                }
                
                if (lowerMessage.Contains("lớp") && (lowerMessage.Contains("hoạt động") || lowerMessage.Contains("tạm dừng")))
                {
                    return await GetClassesByStatusData(message);
                }
                
                if (lowerMessage.Contains("tìm kiếm") || lowerMessage.Contains("search"))
                {
                    return await GetSearchData(message);
                }
                
                return null; // Không cần thêm data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting additional data");
                return null;
            }
        }

        private async Task<object> GetClassByNameData(string message)
        {
            // Extract class name from message
            var classNames = ExtractClassNames(message);
            var results = new List<object>();
            
            foreach (var className in classNames)
            {
                var classData = await _classRepository.GetClassByNameAsync(className);
                if (classData != null)
                {
                    results.Add(new
                    {
                        Type = "ClassByName",
                        ClassName = className,
                        Data = new
                        {
                            Id = classData.Id,
                            Name = classData.Name,
                            Status = classData.Status,
                            TeacherId = classData.TeacherId,
                            MaxStudents = classData.NumberStudent,
                            ScheduleCount = classData.Schedules?.Count ?? 0,
                            ActivityCount = classData.Schedules?.SelectMany(s => s.Activities).Count() ?? 0
                        }
                    });
                }
            }
            
            return new { ClassByNameResults = results };
        }

        private async Task<object> GetTeacherByEmailData(string message)
        {
            // Extract email from message
            var emails = ExtractEmails(message);
            var results = new List<object>();
            
            foreach (var email in emails)
            {
                var teacher = await _accountRepository.GetAccountByEmailAsync(email);
                if (teacher != null && teacher.Role == "TEACHER")
                {
                    results.Add(new
                    {
                        Type = "TeacherByEmail",
                        Email = email,
                        Data = new
                        {
                            Id = teacher.Id,
                            Name = teacher.Name,
                            Email = teacher.Email,
                            Role = teacher.Role,
                            Status = teacher.Status
                        }
                    });
                }
            }
            
            return new { TeacherByEmailResults = results };
        }

        private async Task<object> GetParentsByJobData(string message)
        {
            // Extract job from message
            var jobs = ExtractJobs(message);
            var results = new List<object>();
            
            foreach (var job in jobs)
            {
                var parents = await _parentRepository.GetParentsByJobAsync(job, 5);
                if (parents.Any())
                {
                    results.Add(new
                    {
                        Type = "ParentsByJob",
                        Job = job,
                        Count = parents.Count(),
                        Data = parents.Select(p => new
                        {
                            Id = p.Id,
                            Job = p.Job,
                            RelationshipToChild = p.RelationshipToChild,
                            AccountId = p.AccountId
                        }).ToList()
                    });
                }
            }
            
            return new { ParentsByJobResults = results };
        }

        private async Task<object> GetParentsByRelationshipData(string message)
        {
            var relationships = new List<string>();
            
            if (message.Contains("cha")) relationships.Add("Cha");
            if (message.Contains("mẹ")) relationships.Add("Mẹ");
            
            var results = new List<object>();
            
            foreach (var relationship in relationships)
            {
                var parents = await _parentRepository.GetParentsByRelationshipAsync(relationship, 5);
                if (parents.Any())
                {
                    results.Add(new
                    {
                        Type = "ParentsByRelationship",
                        Relationship = relationship,
                        Count = parents.Count(),
                        Data = parents.Select(p => new
                        {
                            Id = p.Id,
                            Job = p.Job,
                            RelationshipToChild = p.RelationshipToChild,
                            AccountId = p.AccountId
                        }).ToList()
                    });
                }
            }
            
            return new { ParentsByRelationshipResults = results };
        }

        private async Task<object> GetClassesByStatusData(string message)
        {
            var status = "ACTIVE";
            if (message.Contains("tạm dừng") || message.Contains("inactive")) status = "INACTIVE";
            
            var classes = await _classRepository.GetClassesByStatusAsync(status, 10);
            
            return new
            {
                Type = "ClassesByStatus",
                Status = status,
                Count = classes.Count(),
                Data = classes.Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status,
                    TeacherId = c.TeacherId,
                    MaxStudents = c.NumberStudent,
                    ScheduleCount = c.Schedules?.Count ?? 0
                }).ToList()
            };
        }

        private async Task<object> GetSearchData(string message)
        {
            // Extract search term
            var searchTerm = ExtractSearchTerm(message);
            if (string.IsNullOrEmpty(searchTerm)) return new { SearchResults = new List<object>() };
            
            var results = new List<object>();
            
            // Search classes
            var classes = await _classRepository.SearchClassesAsync(searchTerm, 5);
            if (classes.Any())
            {
                results.Add(new
                {
                    Type = "Classes",
                    SearchTerm = searchTerm,
                    Count = classes.Count(),
                    Data = classes.Select(c => new
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Status = c.Status,
                        TeacherId = c.TeacherId
                    }).ToList()
                });
            }
            
            // Search accounts
            var accounts = await _accountRepository.SearchAccountsAsync(searchTerm, 5);
            if (accounts.Any())
            {
                results.Add(new
                {
                    Type = "Accounts",
                    SearchTerm = searchTerm,
                    Count = accounts.Count(),
                    Data = accounts.Select(a => new
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Email = a.Email,
                        Role = a.Role,
                        Status = a.Status
                    }).ToList()
                });
            }
            
            return new { SearchResults = results };
        }

        // Helper methods for extracting data from natural language
        private List<string> ExtractClassNames(string message)
        {
            var classNames = new List<string>();
            var patterns = new[]
            {
                @"lớp\s+([A-Za-z0-9\s]+)",
                @"class\s+([A-Za-z0-9\s]+)",
                @"""([A-Za-z0-9\s]+)"".*lớp"
            };
            
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(message, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        classNames.Add(match.Groups[1].Value.Trim());
                    }
                }
            }
            
            return classNames.Distinct().ToList();
        }

        private List<string> ExtractEmails(string message)
        {
            var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            var matches = Regex.Matches(message, emailPattern);
            return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }

        private List<string> ExtractJobs(string message)
        {
            var jobs = new List<string>();
            var commonJobs = new[] { "giáo viên", "bác sĩ", "kỹ sư", "kinh doanh", "công nhân", "nông dân", "công chức" };
            
            foreach (var job in commonJobs)
            {
                if (message.ToLower().Contains(job))
                {
                    jobs.Add(job);
                }
            }
            
            return jobs;
        }

        private string ExtractSearchTerm(string message)
        {
            var patterns = new[]
            {
                @"tìm\s+(.+?)(?:\s|$)",
                @"search\s+(.+?)(?:\s|$)",
                @"""(.+?)"""
            };
            
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
            
            return string.Empty;
        }

        private async Task<string> GetSystemPromptWithDataAsync(DataContext dataContext, string userRole)
        {
            var systemPrompt = new StringBuilder();
            
            // Base system prompt
            systemPrompt.AppendLine("Bạn là AI Assistant thông minh cho hệ thống PES (Primary Education System).");
            systemPrompt.AppendLine();
            
            // Thêm data context vào system prompt
            if (dataContext.Classes?.Any() == true)
            {
                systemPrompt.AppendLine("=== DỮ LIỆU LỚP HỌC HIỆN TẠI ===");
                foreach (var cls in dataContext.Classes.Take(5))
                {
                    var status = cls.IsActive ? "Đang hoạt động" : "Tạm dừng";
                    systemPrompt.AppendLine($"- Lớp {cls.Name} (ID: {cls.Id}): {cls.CurrentStudents}/{cls.MaxStudents} học sinh, {cls.ScheduleCount} lịch học, {cls.ActivityCount} hoạt động - {status}");
                }
                
                // Thêm phân tích lớp học
                var activeClasses = dataContext.Classes.Where(c => c.IsActive).ToList();
                var maxStudentsClass = dataContext.Classes.OrderByDescending(c => c.CurrentStudents).FirstOrDefault();
                var minStudentsClass = dataContext.Classes.OrderBy(c => c.CurrentStudents).FirstOrDefault();
                
                systemPrompt.AppendLine();
                systemPrompt.AppendLine("=== PHÂN TÍCH LỚP HỌC ===");
                systemPrompt.AppendLine($"- Lớp có nhiều học sinh nhất: {maxStudentsClass?.Name} ({maxStudentsClass?.CurrentStudents} học sinh)");
                systemPrompt.AppendLine($"- Lớp có ít học sinh nhất: {minStudentsClass?.Name} ({minStudentsClass?.CurrentStudents} học sinh)");
                systemPrompt.AppendLine($"- Tổng số lớp đang hoạt động: {activeClasses.Count}");
                systemPrompt.AppendLine();
            }

            if (dataContext.Statistics != null)
            {
                systemPrompt.AppendLine("=== THỐNG KÊ HỆ THỐNG ===");
                systemPrompt.AppendLine($"- Tổng tài khoản: {dataContext.Statistics.TotalAccounts}");
                systemPrompt.AppendLine($"- Giáo viên: {dataContext.Statistics.TeacherCount}");
                systemPrompt.AppendLine($"- Phụ huynh (tài khoản): {dataContext.Statistics.ParentAccountCount}");
                systemPrompt.AppendLine($"- Phụ huynh (thông tin): {dataContext.Statistics.TotalParents}");
                systemPrompt.AppendLine($"- Tổng lớp học: {dataContext.Statistics.TotalClasses}");
                systemPrompt.AppendLine($"- Lớp đang hoạt động: {dataContext.Statistics.ActiveClassesCount}");
                systemPrompt.AppendLine($"- Tổng lịch học: {dataContext.Statistics.TotalSchedules}");
                systemPrompt.AppendLine($"- Tổng hoạt động: {dataContext.Statistics.TotalActivities}");
                systemPrompt.AppendLine();
            }

            // Hiển thị additional data nếu có
            if (dataContext.AdditionalData != null)
            {
                systemPrompt.AppendLine("=== DỮ LIỆU BỔ SUNG (DYNAMIC QUERY) ===");
                systemPrompt.AppendLine(JsonSerializer.Serialize(dataContext.AdditionalData, new JsonSerializerOptions { WriteIndented = true }));
                systemPrompt.AppendLine();
            }

            // Thêm hướng dẫn sử dụng data
            systemPrompt.AppendLine("=== HƯỚNG DẪN SỬ DỤNG DỮ LIỆU ===");
            systemPrompt.AppendLine("1. Sử dụng dữ liệu thực tế từ database để trả lời câu hỏi");
            systemPrompt.AppendLine("2. Đưa ra thông tin chính xác và cập nhật");
            systemPrompt.AppendLine("3. Khi không có dữ liệu, hãy nói rõ và đề xuất cách lấy thông tin");
            systemPrompt.AppendLine("4. Ưu tiên dữ liệu mới nhất và chính xác nhất");
            systemPrompt.AppendLine();
            
            // Thêm hướng dẫn trả lời câu hỏi cụ thể về dữ liệu
            systemPrompt.AppendLine("=== CÁCH TRẢ LỜI CÂU HỎI VỀ DỮ LIỆU ===");
            systemPrompt.AppendLine("Khi người dùng hỏi về:");
            systemPrompt.AppendLine("- 'Có bao nhiêu role/loại tài khoản?': Trả lời: Có 3 loại tài khoản chính - Giáo viên ({TeacherCount}), Phụ huynh ({ParentAccountCount}), và các role khác");
            systemPrompt.AppendLine("- 'Có bao nhiêu lớp học?': Trả lời: Hiện có {TotalClasses} lớp học, trong đó {ActiveClassesCount} lớp đang hoạt động");
            systemPrompt.AppendLine("- 'Lớp nào có nhiều học sinh nhất?': So sánh CurrentStudents của các lớp trong danh sách");
            systemPrompt.AppendLine("- 'Có bao nhiêu giáo viên?': Trả lời: Hiện có {TeacherCount} giáo viên trong hệ thống");
            systemPrompt.AppendLine("- 'Có bao nhiêu phụ huynh?': Trả lời: Có {ParentAccountCount} tài khoản phụ huynh và {TotalParents} thông tin phụ huynh");
            systemPrompt.AppendLine("- 'Lớp nào hoạt động?': Liệt kê các lớp có IsActive = true");
            systemPrompt.AppendLine("- 'Có bao nhiêu hoạt động?': Trả lời: Hiện có {TotalActivities} hoạt động trong hệ thống");
            systemPrompt.AppendLine("- 'Lịch học như thế nào?': Mô tả dựa trên ScheduleData, có {TotalSchedules} lịch học");
            systemPrompt.AppendLine("- 'Học phí trung bình': Nếu có dữ liệu học phí, tính trung bình; nếu không có thì nói rõ 'Hệ thống chưa có thông tin học phí'");
            systemPrompt.AppendLine("- 'Thống kê tổng quan': Tổng hợp tất cả số liệu trên");
            systemPrompt.AppendLine();
            
            // Thêm hướng dẫn sử dụng Dynamic Data
            systemPrompt.AppendLine("=== HƯỚNG DẪN SỬ DỤNG DYNAMIC DATA ===");
            systemPrompt.AppendLine("Nếu có DỮ LIỆU BỔ SUNG (DYNAMIC QUERY) ở trên:");
            systemPrompt.AppendLine("1. Sử dụng dữ liệu này để trả lời câu hỏi cụ thể của người dùng");
            systemPrompt.AppendLine("2. Phân tích và giải thích dữ liệu một cách chi tiết");
            systemPrompt.AppendLine("3. Đưa ra insights và nhận xét dựa trên dữ liệu thực tế");
            systemPrompt.AppendLine("4. Nếu có nhiều kết quả, hãy so sánh và đưa ra top results");
            systemPrompt.AppendLine("5. Luôn trích dẫn số liệu cụ thể từ dữ liệu");
            systemPrompt.AppendLine();

            // Thêm hướng dẫn theo vai trò
            systemPrompt.AppendLine($"=== HƯỚNG DẪN CHO {userRole.ToUpper()} ===");
            if (userRole == "PARENT")
            {
                systemPrompt.AppendLine("Bạn đang hỗ trợ phụ huynh. Tập trung vào:");
                systemPrompt.AppendLine("- Quản lý thông tin con em");
                systemPrompt.AppendLine("- Đăng ký nhập học");
                systemPrompt.AppendLine("- Thanh toán học phí");
                systemPrompt.AppendLine("- Theo dõi học tập");
            }
            else if (userRole == "TEACHER")
            {
                systemPrompt.AppendLine("Bạn đang hỗ trợ giáo viên. Tập trung vào:");
                systemPrompt.AppendLine("- Quản lý lớp học");
                systemPrompt.AppendLine("- Lịch dạy");
                systemPrompt.AppendLine("- Hoạt động học tập");
            }
            else if (userRole == "HR")
            {
                systemPrompt.AppendLine("Bạn đang hỗ trợ nhân sự. Tập trung vào:");
                systemPrompt.AppendLine("- Quản lý tài khoản");
                systemPrompt.AppendLine("- Tuyển dụng");
                systemPrompt.AppendLine("- Báo cáo thống kê");
            }

            return systemPrompt.ToString();
        }

        private string ProcessMarkdownAndSpecialChars(string text)
        {
            try
            {
                // 1. Xử lý ký tự đặc biệt trước
                text = FixSpecialCharacters(text);
                
                // 2. Xử lý markdown (đơn giản hóa)
                text = ProcessSimpleMarkdown(text);
                
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing markdown and special characters");
                return text; // Return original text if error
            }
        }

        private string FixSpecialCharacters(string text)
        {
            // Fix common Vietnamese character issues
            var fixes = new Dictionary<string, string>
            {
                { "à", "à" }, { "á", "á" }, { "ả", "ả" }, { "ã", "ã" }, { "ạ", "ạ" },
                { "ă", "ă" }, { "ằ", "ằ" }, { "ắ", "ắ" }, { "ẳ", "ẳ" }, { "ẵ", "ẵ" }, { "ặ", "ặ" },
                { "â", "â" }, { "ầ", "ầ" }, { "ấ", "ấ" }, { "ẩ", "ẩ" }, { "ẫ", "ẫ" }, { "ậ", "ậ" },
                { "è", "è" }, { "é", "é" }, { "ẻ", "ẻ" }, { "ẽ", "ẽ" }, { "ẹ", "ẹ" },
                { "ê", "ê" }, { "ề", "ề" }, { "ế", "ế" }, { "ể", "ể" }, { "ễ", "ễ" }, { "ệ", "ệ" },
                { "ì", "ì" }, { "í", "í" }, { "ỉ", "ỉ" }, { "ĩ", "ĩ" }, { "ị", "ị" },
                { "ò", "ò" }, { "ó", "ó" }, { "ỏ", "ỏ" }, { "õ", "õ" }, { "ọ", "ọ" },
                { "ô", "ô" }, { "ồ", "ồ" }, { "ố", "ố" }, { "ổ", "ổ" }, { "ỗ", "ỗ" }, { "ộ", "ộ" },
                { "ơ", "ơ" }, { "ờ", "ờ" }, { "ớ", "ớ" }, { "ở", "ở" }, { "ỡ", "ỡ" }, { "ợ", "ợ" },
                { "ù", "ù" }, { "ú", "ú" }, { "ủ", "ủ" }, { "ũ", "ũ" }, { "ụ", "ụ" },
                { "ư", "ư" }, { "ừ", "ừ" }, { "ứ", "ứ" }, { "ử", "ử" }, { "ữ", "ữ" }, { "ự", "ự" },
                { "ỳ", "ỳ" }, { "ý", "ý" }, { "ỷ", "ỷ" }, { "ỹ", "ỹ" }, { "ỵ", "ỵ" },
                { "đ", "đ" }, { "Đ", "Đ" }
            };

            foreach (var fix in fixes)
            {
                text = text.Replace(fix.Key, fix.Value);
            }

            // Fix common encoding issues
            text = text.Replace("Ã¡", "á");
            text = text.Replace("Ã ", "à");
            text = text.Replace("Ã¢", "â");
            text = text.Replace("Ã£", "ã");
            text = text.Replace("Ã¤", "ä");
            text = text.Replace("Ã¥", "å");
            text = text.Replace("Ã¦", "æ");
            text = text.Replace("Ã§", "ç");
            text = text.Replace("Ã¨", "è");
            text = text.Replace("Ã©", "é");
            text = text.Replace("Ãª", "ê");
            text = text.Replace("Ã«", "ë");
            text = text.Replace("Ã¬", "ì");
            text = text.Replace("Ã", "í");
            text = text.Replace("Ã®", "î");
            text = text.Replace("Ã¯", "ï");
            text = text.Replace("Ã°", "ð");
            text = text.Replace("Ã±", "ñ");
            text = text.Replace("Ã²", "ò");
            text = text.Replace("Ã³", "ó");
            text = text.Replace("Ã´", "ô");
            text = text.Replace("Ãµ", "õ");
            text = text.Replace("Ã¶", "ö");
            text = text.Replace("Ã·", "÷");
            text = text.Replace("Ã¸", "ø");
            text = text.Replace("Ã¹", "ù");
            text = text.Replace("Ãº", "ú");
            text = text.Replace("Ã»", "û");
            text = text.Replace("Ã¼", "ü");
            text = text.Replace("Ã½", "ý");
            text = text.Replace("Ã¾", "þ");
            text = text.Replace("Ã¿", "ÿ");

            return text;
        }

        private string ProcessSimpleMarkdown(string text)
        {
            try
            {
                // Simple markdown processing without external library
                // Convert **bold** to bold
                text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");
                
                // Convert *italic* to italic
                text = Regex.Replace(text, @"\*(.*?)\*", "$1");
                
                // Convert ## headers to plain text
                text = Regex.Replace(text, @"^##\s+(.*)$", "$1", RegexOptions.Multiline);
                
                // Convert # headers to plain text
                text = Regex.Replace(text, @"^#\s+(.*)$", "$1", RegexOptions.Multiline);
                
                // Convert - list items to plain text
                text = Regex.Replace(text, @"^-\s+(.*)$", "• $1", RegexOptions.Multiline);
                
                // Convert numbered lists
                text = Regex.Replace(text, @"^\d+\.\s+(.*)$", "$1", RegexOptions.Multiline);
                
                // Clean up extra whitespace
                text = Regex.Replace(text, @"\n\s*\n", "\n\n");
                
                return text.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing simple markdown");
                return text; // Return original text if markdown processing fails
            }
        }
    }

    // ===== DATA CLASSES =====

    public class DataContext
    {
        public List<ClassData> Classes { get; set; } = new();
        public List<TeacherData> Teachers { get; set; } = new();
        public List<ParentData> Parents { get; set; } = new();
        public List<ScheduleData> Schedules { get; set; } = new();
        public List<ActivityData> Activities { get; set; } = new();
        public StatisticsData Statistics { get; set; } = new();
        public object UserSpecificData { get; set; }
        public object AdditionalData { get; set; }
    }

    public class ClassData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }
        public bool IsActive { get; set; }
        public int ScheduleCount { get; set; }
        public int ActivityCount { get; set; }
    }

    public class TeacherData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ParentData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Job { get; set; } = string.Empty;
        public string RelationshipToChild { get; set; } = string.Empty;
    }

    public class ScheduleData
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int ActivityCount { get; set; }
    }

    public class ActivityData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class StatisticsData
    {
        public int TotalAccounts { get; set; }
        public int TotalParents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSchedules { get; set; }
        public int TotalActivities { get; set; }
        public int TeacherCount { get; set; }
        public int ParentAccountCount { get; set; }
        public int ActiveClassesCount { get; set; }
    }

    // Helper classes for Gemini API response
    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; } = Array.Empty<Candidate>();
    }

    public class Candidate
    {
        public Content content { get; set; } = new();
    }

    public class Content
    {
        public Part[] parts { get; set; } = Array.Empty<Part>();
    }

    public class Part
    {
        public string text { get; set; } = string.Empty;
    }
}
